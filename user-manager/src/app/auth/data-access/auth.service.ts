import { computed, inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { filter, Subject, Subscription, switchMap, tap, timer } from "rxjs";
import { mapJwtToUser } from "../utils/jwt.utils";
import { LoginRequest } from "../interfaces/requests/login.request";
import { LoginResponse } from "../interfaces/responses/login.response";
import { LoggedInUser } from "../interfaces/logged-in-user";
import { Router } from "@angular/router";

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
        this.#state.update((state) => ({ ...state, user: undefined }));
        this.#router.navigate(['/login']);
      });
  }

  private scheduleRefresh(expiresAt: Date) {
    const delay = expiresAt.getTime() - 60_000 - Date.now();
    if (delay > 0) {
      this.refreshTimerSub?.unsubscribe();
      this.refreshTimerSub = timer(delay).subscribe(() => this.refresh$.next());
    }
  }

  private clearRefreshTimer() {
    this.refreshTimerSub?.unsubscribe();
    this.refreshTimerSub = undefined;
  }
}