import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthStore } from '../auth/auth.store';
import { environment } from '../../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(AuthStore);

  // Prepend base URL if relative
  const url = req.url.startsWith('http') ? req.url : `${environment.apiUrl}${req.url}`;

  let headers = req.headers.set('Content-Type', 'application/json');

  const token = store.accessToken();
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  return next(req.clone({ url, headers }));
};
