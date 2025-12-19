import { AuthGuardData, createAuthGuard } from 'keycloak-angular';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { inject } from '@angular/core';

const isAccessAllowed = async (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
  authData: AuthGuardData
): Promise<boolean | UrlTree> => {
  const { authenticated, grantedRoles } = authData;
  const router = inject(Router);

  // 1. Si no está autenticado, redirigir al login de Keycloak
  if (!authenticated) {
    // Aquí puedes disparar el login automáticamente si prefieres
    return router.parseUrl('/login'); 
  }

  // 2. Obtener el rol (o lista de roles) requerido desde la ruta
  const requiredRole = route.data['role'];

  // Si la ruta no tiene definición de roles, permitimos el paso por estar autenticado
  if (!requiredRole) {
    return true;
  }

  // 3. Lógica de validación robusta (Busca en Realm Roles y Resource Roles)
  const hasRole = (role: string): boolean => {
    // Busca en roles globales (Donde tienes tu "admin")
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