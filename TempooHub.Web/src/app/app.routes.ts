import { Routes } from '@angular/router';
import { DashboardPage } from './dashboard-page/dashboard-page';
import { LayoutComponent } from './shared/layout/layout';
import { AdminPanelPage } from './admin-panel/admin-panel';
import { APP_ROUTES } from './shared/constants/navigation-routes';
import { UserManagment } from './admin-panel/user-managment/user-managment';
import { LoginComponent } from './auth/login-component/login-component';
import { canActivateAuthRole } from './shared/guards/auth-role-guard';

export const routes: Routes = [
    {
        path: '',
        component: LayoutComponent,
        canActivate: [canActivateAuthRole],
        children: [
            {
                path: APP_ROUTES.HOME,
                component: DashboardPage
            },
            {
                path: APP_ROUTES.ADMIN,
                component: AdminPanelPage,
                data: { role: 'admin' }
            },
            {
                path: `${APP_ROUTES.ADMIN}/${APP_ROUTES.USER_MANAGEMENT}`,
                data: { role: 'test-role' },
                component: UserManagment
            }
        ]
    }
];
