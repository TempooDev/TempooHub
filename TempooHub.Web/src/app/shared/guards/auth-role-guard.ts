import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth.service';

export const canActivateAuthRole: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();
  const requiredRole = route.data['role'];

  if (!user) {
    return router.parseUrl('/login');
  }

  if (!requiredRole) return true;

  const userRoles: string[] = user.roles || [];
  
  if (userRoles.includes(requiredRole)) {
    return true;
  }

  return router.parseUrl('/forbidden');
};