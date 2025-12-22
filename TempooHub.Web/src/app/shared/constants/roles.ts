
export const ROLES = {
    ADMIN: 'Admin',
    USER_MANAGEMENT: 'user-management',
    BASIC: 'basic-user',
    PREMIUM: 'premium-user',
    ENTERPRISE: 'enterprise-user'
};
export type RouteKeys = keyof typeof ROLES;