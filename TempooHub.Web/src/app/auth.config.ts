import { APP_INITIALIZER, Provider } from '@angular/core';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';

const authConfig: AuthConfig = {
  issuer: 'http://tempoohub-auth:5001',
  redirectUri: window.location.origin + '/',
  silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  clientId: 'tempoohub-web',
  responseType: 'code',
  scope: 'openid profile email api',
  useRefreshToken: true,
  requireHttps: false
};

export function initializeAuth(oauth: OAuthService) {
  return async () => {
    oauth.configure(authConfig);
    oauth.setupAutomaticSilentRefresh();
    await oauth.loadDiscoveryDocumentAndTryLogin();
  };
}

export const authProviders: Provider[] = [
  OAuthService,
  {
    provide: APP_INITIALIZER,
    useFactory: initializeAuth,
    deps: [OAuthService],
    multi: true
  }
];
