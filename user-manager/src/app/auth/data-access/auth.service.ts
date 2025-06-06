import { computed, inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { filter, Subject, Subscription, switchMap, tap, timer } from "rxjs";
import { mapJwtToUser } from "../utils/jwt.utils";
import { LoginRequest } from "../interfaces/requests/login.request";
import { LoginResponse } from "../interfaces/responses/login.response";
import { LoggedInUser } from "../interfaces/logged-in-user";
import { Router } from "@angular/router";
import { clearUserFromSessionStorage, getUserFromSessionStorage, setUserInSessionStorage } from "../utils/user-session-storage.utils";

type AuthState = {
  user?: LoggedInUser;
  isLoading: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  readonly #http = inject(HttpClient);
  readonly #router = inject(Router);
  private refreshTimerSub?: Subscription;

  readonly #state = signal<AuthState>({
    isLoading: false
  });

  user = computed(() => this.#state().user);
  isAuthenticated = computed(() => !!this.#state().user);
  isLoading = computed(() => this.#state().isLoading);

  readonly login$ = new Subject<LoginRequest>();
  readonly logout$ = new Subject<void>();
  readonly refresh$ = new Subject<void>();

  readonly #login = this.login$
    .pipe(
      tap(() => this.#state.update((state) => ({ ...state, isLoading: true }))),
      switchMap((req) => this.#http.post<LoginResponse>('/auth/login', {
          email: req.email,
          password: req.password
        })
      ),
      tap((res) => {
        const user = mapJwtToUser(res.accessToken, res.refreshToken);
        this.#state.update((state) => ({ ...state, isLoading: false, user }));
        setUserInSessionStorage(user);
        this.scheduleRefresh(user.expiresAt);
        this.#router.navigate(['/']);
      })
    );

  readonly #refresh = this.refresh$
    .pipe(
      filter(() => !!this.#state().user),
      switchMap(() => this.#http.post<LoginResponse>('/auth/refresh', { refreshToken: this.#state().user?.refreshToken })),
      tap((res) => {
        const user = mapJwtToUser(res.accessToken, res.refreshToken);
        this.#state.update((state) => ({ ...state, user }));
        setUserInSessionStorage(user);
        this.scheduleRefresh(user.expiresAt);
      })
    );

  constructor() {
    this.#login.subscribe();
    this.#refresh.subscribe()

    this.logout$
      .pipe(
        switchMap(() => this.#http.post('/auth/revoke', { refreshToken: this.#state().user?.refreshToken })),
      )
      .subscribe(() => {
        this.clearRefreshTimer();
        clearUserFromSessionStorage();
        this.#state.update((state) => ({ ...state, user: undefined }));
        this.#router.navigate(['/login']);
      });

    this.setUserFromSession();
  }

  private scheduleRefresh(expiresAt: Date) {
    this.clearRefreshTimer();
    const now = Date.now();
    const refreshTime = expiresAt.getTime() - 60_000;
    const delay = refreshTime - now;

    if (delay > 0) {
      this.refreshTimerSub = timer(delay).subscribe(() => this.refresh$.next());
    } else if (expiresAt.getTime() > now) {
      timer(0).subscribe(() => this.refresh$.next());
  } else {
      this.logout$.next();
    }
  }

  private clearRefreshTimer() {
    this.refreshTimerSub?.unsubscribe();
    this.refreshTimerSub = undefined;
  }

  private setUserFromSession() {
    const user = getUserFromSessionStorage();
    if (user) {
      const expiresAt = new Date(user.expiresAt);
      if (expiresAt > new Date()) {
        user.expiresAt = expiresAt;
        this.#state.update((state) => ({ ...state, user }));
        this.scheduleRefresh(expiresAt);
      } else {
        clearUserFromSessionStorage();
      }
    }
  }
}