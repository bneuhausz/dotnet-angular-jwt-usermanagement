import { Component, inject } from '@angular/core';
import { AuthService } from './auth/data-access/auth.service';
import { LayoutComponent } from './shared/ui/layout.component';

@Component({
  selector: 'app-root',
  imports: [LayoutComponent],
  template: `
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
