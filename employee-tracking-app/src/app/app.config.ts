import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideNativeDateAdapter } from '@angular/material/core';

import { routes } from './app.routes';
import { apiInterceptor } from './core/http/api.interceptor';
import { tokenRefreshInterceptor } from './core/auth/token-refresh.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';
import { loadingInterceptor } from './core/http/loading.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideAnimations(),
    provideNativeDateAdapter(),
    provideHttpClient(
      withInterceptors([
        apiInterceptor,
        tokenRefreshInterceptor,
        errorInterceptor,
        loadingInterceptor
      ])
    )
  ]
};
