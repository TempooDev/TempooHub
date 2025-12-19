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

  const hasRole = (role: string): boolean => {
    const hasRealmRole = grantedRoles.realmRoles.includes(role);
    
    // Busca en roles de cliente (account, tempoo-hub-client, etc)
    const hasResourceRole = Object.values(grantedRoles.resourceRoles)
      .some((roles) => roles.includes(role));

    return hasRealmRole || hasResourceRole;
  };

  // 4. Verificación final
  if (hasRole(requiredRole)) {
    return true;
  }

  // Si está autenticado pero no tiene el rol, mandamos a Forbidden
  return router.parseUrl('/forbidden');
};

export const canActivateAuthRole = createAuthGuard<CanActivateFn>(isAccessAllowed);