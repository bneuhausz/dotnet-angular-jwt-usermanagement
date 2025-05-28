import { computed, inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Subject, switchMap, tap } from "rxjs";
import { mapJwtToUser } from "../utils/jwt.utils";
import { LoginRequest } from "../interfaces/requests/login.request";
import { LoginResponse } from "../interfaces/responses/login.response";
import { User } from "../interfaces/user";
import { Router } from "@angular/router";

type AuthState = {
  user?: User;
  isLoading: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  readonly #http = inject(HttpClient);
  readonly #router = inject(Router);

  readonly #state = signal<AuthState>({
    isLoading: false
  });

  user = computed(() => this.#state().user);
  isAuthenticated = computed(() => !!this.#state().user);
  isLoading = computed(() => this.#state().isLoading);

  readonly login$ = new Subject<LoginRequest>();
  readonly logout$ = new Subject<void>();

  readonly #login = this.login$
    .pipe(
      tap(() => this.#state.update((state) => ({ ...state, isLoading: true }))),
      switchMap((req) => this.#http.post<LoginResponse>('/auth/login', {
          email: req.email,
          password: req.password
        })
      ),
      tap((res) => {
        const user = mapJwtToUser(res.token);
        console.log('Mapped user:', user);
        this.#state.update((state) => ({ ...state, isLoading: false, user }));
        this.#router.navigate(['/']);
      })
    );

  constructor() {
    this.#login.subscribe();

    this.logout$
      .subscribe(() => {
        this.#state.update((state) => ({ ...state, user: undefined }));
        this.#router.navigate(['/login']);
      });
  }
}