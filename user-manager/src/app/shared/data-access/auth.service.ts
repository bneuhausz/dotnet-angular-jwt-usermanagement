import { computed, inject, Injectable, signal } from "@angular/core";
import { User } from "../interfaces/user";
import { HttpClient } from "@angular/common/http";
import { LoginResponse } from "../interfaces/responses/login.response";
import { Subject, switchMap, tap } from "rxjs";
import { LoginRequest } from "../interfaces/requests/login.request";
import { mapJwtToUser } from "../utils/jwt.utils";

type AuthState = {
  user?: User;
  isLoading: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  readonly #http = inject(HttpClient);

  readonly #state = signal<AuthState>({
    isLoading: false
  });

  user = computed(() => this.#state().user);
  isAuthenticated = computed(() => !!this.#state().user);
  isLoading = computed(() => this.#state().isLoading);

  readonly login$ = new Subject<LoginRequest>();

  readonly #login = this.login$
    .pipe(
      tap(() => this.#state.update((state) => ({ ...state, isLoading: true }))),
      switchMap((req) => this.#http.post<LoginResponse>('/auth/login', {
          email: req.email,
          password: req.password
        })
      ),
      tap((res) => {
        console.log('Login successful:', res);
        const user = mapJwtToUser(res.token);
        console.log('Mapped user:', user);
        this.#state.update((state) => ({ ...state, isLoading: false, user }));
      })
    );

  constructor() {
    this.#login.subscribe();
  }
}