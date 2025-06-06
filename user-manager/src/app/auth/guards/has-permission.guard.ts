import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../data-access/auth.service';

export const hasPermissionGuard = (requiredPermission: string): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.user()?.permissions.includes(requiredPermission)
      ? true
      : router.createUrlTree(['/unauthorized']);
  };
};