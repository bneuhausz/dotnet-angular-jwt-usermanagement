import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { Menu } from '../../auth/interfaces/menu';
import { MatSidenav } from '@angular/material/sidenav';

export interface MenuItem {
  Name: string;
  SubMenus: MenuItem[];
}

@Component({
  selector: 'app-dynamic-menu',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatListModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
  ],
  template: `
    @for (menu of menus(); track menu.name) {
      @if (menu.subMenus.length > 0) {
        <a mat-list-item [matMenuTriggerFor]="menuRef">
          <mat-icon matListItemIcon>person_add</mat-icon>
          <span>{{ menu.name }}</span>
        </a>

        <mat-menu #menuRef="matMenu" [overlapTrigger]="false" style="width: 200px;">
          <app-dynamic-menu [menus]="menu.subMenus" [sidenav]="sidenav()" />
        </mat-menu>
      }
      @else {
        <a mat-list-item [routerLink]="getRouterLink(menu.name)" (click)="sidenav().close()">
          <mat-icon matListItemIcon>person_add</mat-icon>
          <span>{{ menu.name }}</span>
        </a>
      }
    }
  `,
})
export class DynamicMenuComponent {
  menus = input.required<Menu[]>();
  sidenav = input.required<MatSidenav>();

  getRouterLink(name: string): string {
    return name.toLowerCase().replace(/\s+/g, '-');
  }
}