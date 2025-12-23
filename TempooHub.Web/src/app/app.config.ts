import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
// 1. Importa withInterceptors y tu función interceptora
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthService } from './shared/auth.service';
import { authInterceptor } from './auth.interceptor';

function initializeAuth(authService: AuthService) {
  return () => {
    const token = localStorage.getItem('token');
    if (token) {
      return authService.checkStatus();
    }
    return Promise.resolve();
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideNoopAnimations(),
    provideHttpClient(
      withInterceptors([authInterceptor]) 
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAuth,
      deps: [AuthService],
      multi: true
    }
  ]
};