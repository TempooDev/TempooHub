import { Routes } from '@angular/router';
import { DashboardPage } from './dashboard-page/dashboard-page';
import { LayoutComponent } from './shared/layout/layout';
import { AdminPanelPage } from './admin-panel/admin-panel';
import { APP_ROUTES } from './shared/constants/navigation-routes';
import { UserManagment } from './admin-panel/user-managment/user-managment';
import { canActivateAuthRole } from './shared/guards/auth-role-guard';
import { ROLES } from './shared/constants/roles';

export const routes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./auth/login-component/login-component').then(m => m.LoginComponent)
    },
    {
        path: '',
        component: LayoutComponent,
        canActivateChild: [canActivateAuthRole],
        children: [
            {
                path: APP_ROUTES.HOME,
                component: DashboardPage,
                pathMatch: 'full'
            },
            {
                path: APP_ROUTES.ADMIN,
                component: AdminPanelPage,
                data: { role: ROLES.ADMIN }
            },
            {
                path: `${APP_ROUTES.ADMIN}/${APP_ROUTES.USER_MANAGEMENT}`,
                data: { role: ROLES.USER_MANAGEMENT },
                component: UserManagment
            }
        ]
    },
    {
        path: 'forbidden',
        loadComponent: () => import('./auth/forbidden/forbidden').then(m => m.ForbiddenComponent)
    }
];
