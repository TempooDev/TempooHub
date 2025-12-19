import { AuthGuardData, createAuthGuard } from 'keycloak-angular';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { inject } from '@angular/core';
import Keycloak from 'keycloak-js';

const isAccessAllowed = async (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
  authData: AuthGuardData
): Promise<boolean | UrlTree> => {
  const { authenticated, grantedRoles } = authData;
  const router = inject(Router);
  const keycloak = inject(Keycloak);

  if (!authenticated) {
    await keycloak.login({ redirectUri: window.location.origin + state.url });
    return false;
  }

  const requiredRole = route.data['role'];

  if (!requiredRole) {
    return true;
  }

  // build role lists from tokenParsed as a fallback (some setups expose roles under token.resource_access)
  const tokenParsed = (keycloak.tokenParsed as any) || {};
  const tokenRealmRoles: string[] = tokenParsed.realm_access?.roles || [];
  const tokenResourceRoles: Record<string, string[]> = {};
  if (tokenParsed.resource_access) {
    Object.keys(tokenParsed.resource_access).forEach((k) => {
      tokenResourceRoles[k] = tokenParsed.resource_access[k].roles || [];
    });
  }

  const hasRole = (role: string): boolean => {
    const hasRealmRole = (grantedRoles?.realmRoles?.includes(role)) ?? tokenRealmRoles.includes(role);

    const hasResourceRoleFromGranted = grantedRoles?.resourceRoles
      ? Object.values(grantedRoles.resourceRoles).some((roles) => roles.includes(role))
      : false;

    const hasResourceRoleFromToken = Object.values(tokenResourceRoles).some((roles) => roles.includes(role));

    const hasResourceRole = hasResourceRoleFromGranted || hasResourceRoleFromToken;

    console.debug('[AuthGuard] requiredRole=', requiredRole, 'checkRole=', role, { hasRealmRole, hasResourceRole, tokenRealmRoles, tokenResourceRoles, grantedRoles });

    return hasRealmRole || hasResourceRole;
  };

  if (hasRole(requiredRole)) {
    return true;
  }

  return router.parseUrl('/forbidden');
};

export const canActivateAuthRole = createAuthGuard<CanActivateFn>(isAccessAllowed);