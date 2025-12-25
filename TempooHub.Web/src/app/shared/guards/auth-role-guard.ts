import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth.service';

export const canActivateAuthClaim: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();
  const requiredClaimType = route.data['claimType'];
  const requiredClaimValue = route.data['claimValue'];

  if (!user) {
    return router.parseUrl('/login');
  }

  if (!requiredClaimType) return true;

  if (user.claims && user.claims.some(claim => claim.type === requiredClaimType && (!requiredClaimValue || claim.value === requiredClaimValue))) {
    return true;
  }

  return router.parseUrl('/forbidden');
};