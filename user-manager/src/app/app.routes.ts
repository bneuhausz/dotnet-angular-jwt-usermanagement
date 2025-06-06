import { Routes } from '@angular/router';
import { isAuthenticatedGuard } from './auth/guards/auth.guard';
import { hasPermissionGuard } from './auth/guards/has-permission.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/login.component'),
  },
  {
    path: 'home',
    canActivate: [isAuthenticatedGuard()],
    loadComponent: () => import('./home/home.component'),
  },
  {
    path: 'maintenance',
    canActivate: [isAuthenticatedGuard(), hasPermissionGuard('Maintenance')],
    children: [
      {
        path: 'roles',
        canActivate: [hasPermissionGuard('Roles')],
        loadComponent: () => import('./maintenance/roles/roles.component'),
      },
      {
        path: 'users',
        canActivate: [hasPermissionGuard('Users')],
        loadComponent: () => import('./maintenance/users/users.component'),
      },
    ]
  },
  {
    path: 'unauthorized',
    canActivate: [isAuthenticatedGuard()],
    loadComponent: () => import('./unauthorized/unauthorized.component'),
  },
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
];
