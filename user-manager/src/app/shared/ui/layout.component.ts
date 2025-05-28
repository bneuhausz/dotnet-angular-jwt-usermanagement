import { Component, inject } from "@angular/core";
import { HeaderComponent } from "./header.component";
import { RouterLink, RouterOutlet } from "@angular/router";
import { MatSidenavModule } from "@angular/material/sidenav";
import { MatListModule } from "@angular/material/list";
import { MatIconModule } from "@angular/material/icon";
import { AuthService } from "../../auth/data-access/auth.service";

@Component({
  selector: "app-layout",
  template: `
    <mat-sidenav-container fullscreen>
      <mat-sidenav #sidenav>
        <mat-nav-list (click)="sidenav.close()">
          <a mat-list-item routerLink="/">
            <mat-icon matListItemIcon>home</mat-icon>
            <span>Home</span>
          </a>

          <!-- @if (authService.isAuthenticated() && authService.isUserManager()) {
            <a mat-list-item routerLink="user-management">
              <mat-icon matListItemIcon>person_add</mat-icon>
              <span>User Management</span>
            </a>
          } -->
        </mat-nav-list>
      </mat-sidenav>
      <app-header [sidenav]="sidenav"></app-header>
      <main>
        <router-outlet></router-outlet>
      </main>
    </mat-sidenav-container>
  `,
  styles: [`
    main {
      padding-top: 20px;
    }
  `],
  imports: [HeaderComponent, RouterOutlet, MatSidenavModule, MatListModule, RouterLink, MatIconModule],
})
export class LayoutComponent {
  authService = inject(AuthService);
}