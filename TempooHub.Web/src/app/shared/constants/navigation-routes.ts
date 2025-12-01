import { MenuItem } from "primeng/api";

export const APP_ROUTES = {
    HOME: '',
    ADMIN: 'admin',
    USER_MANAGEMENT: 'admin'
};
export type RouteKeys = keyof typeof APP_ROUTES;

export function getPath(key: RouteKeys): string {
    return `/${APP_ROUTES[key]}`;
}


export const ROUTES: MenuItem[] = [
    {
        label: 'Home',
        routerLink: getPath('HOME'),
        icon: 'pi pi-home'
    },
    {
        label: 'Admin',
        routerLink: getPath('ADMIN'),
        icon: 'pi pi-cog',
        items: [
            {
                label: 'User Management',
                routerLink: getPath('USER_MANAGEMENT'),
                icon: 'pi pi-users'
            }
        ]
    }
];