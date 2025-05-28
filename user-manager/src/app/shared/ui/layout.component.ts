import { Component, inject } from "@angular/core";
import { HeaderComponent } from "./header.component";
import { RouterLink, RouterOutlet } from "@angular/router";
import { MatSidenavModule } from "@angular/material/sidenav";
import { MatListModule } from "@angular/material/list";
import { MatIconModule } from "@angular/material/icon";
import { AuthService } from "../../auth/data-access/auth.service";
import { DynamicMenuComponent } from "./dynamic-menu.component";

@Component({
  selector: "app-layout",
  template: `
    <mat-sidenav-container fullscreen>
      <mat-sidenav #sidenav>
        <mat-nav-list>
          <a mat-list-item routerLink="/" (click)="sidenav.close()">
            <mat-icon matListItemIcon>home</mat-icon>
            <span>Home</span>
          </a>

          @if (authService.isAuthenticated()) {
            <app-dynamic-menu [menus]="authService.user()!.menus" [sidenav]="sidenav" />
          }
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
  imports: [HeaderComponent, RouterOutlet, MatSidenavModule, MatListModule, RouterLink, MatIconModule, DynamicMenuComponent],
})
export class LayoutComponent {
  authService = inject(AuthService);
}