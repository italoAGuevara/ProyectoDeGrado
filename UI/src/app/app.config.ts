import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';
import { lastValueFrom } from 'rxjs';
import { AuthService } from './services/auth.service';

export function loadRequireAuth(auth: AuthService) {
  return () => lastValueFrom(auth.loadRequireAuthFromApi()).catch(() => undefined);
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: APP_INITIALIZER, useFactory: loadRequireAuth, deps: [AuthService], multi: true },
  ],
};
