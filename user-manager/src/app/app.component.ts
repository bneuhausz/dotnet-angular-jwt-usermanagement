import { Component, inject } from '@angular/core';
import { AuthService } from './auth/data-access/auth.service';
import { LayoutComponent } from './shared/ui/layout.component';

@Component({
  selector: 'app-root',
  imports: [LayoutComponent],
  template: `
    <!-- <h1>Welcome!</h1>
    <button (click)="authService.login$.next(user)">Login</button>

    <p>
      {{ authService.isAuthenticated() ? 'You are logged in' : 'You are not logged in' }}
    </p>

    <router-outlet /> -->
    <app-layout />
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
