import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './shared/data-access/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <h1>Welcome!</h1>
    <button (click)="authService.login$.next(user)">Login</button>

    <p>
      {{ authService.isAuthenticated() ? 'You are logged in' : 'You are not logged in' }}
    </p>

    <router-outlet />
  `,
  styles: [],
})
export class AppComponent {
  authService = inject(AuthService);
  user = {
    email: 'admin@example.com',
    password: 'admin'
  }
}
