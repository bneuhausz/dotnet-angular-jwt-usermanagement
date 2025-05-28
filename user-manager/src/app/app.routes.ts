import { Routes } from '@angular/router';
import { isAuthenticatedGuard } from './auth/guards/auth.guard';

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
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
];
