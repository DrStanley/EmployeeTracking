import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { AuthStore } from '../auth/auth.store';

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<string | null>(null);

export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const store       = inject(AuthStore);

  // Skip refresh endpoint itself
  if (req.url.includes('/auth/refresh') || req.url.includes('/auth/login')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) return throwError(() => error);

      const refreshToken = store.refreshToken();
      const accessToken  = store.accessToken();
      if (!refreshToken || !accessToken) {
        authService.logout();
        return throwError(() => error);
      }

      if (isRefreshing) {
        // Queue this request until refresh completes
        return refreshSubject.pipe(
          filter(t => t !== null),
          take(1),
          switchMap(newToken => {
            return next(req.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` }
            }));
          })
        );
      }

      isRefreshing = true;
      refreshSubject.next(null);

      return authService.refresh({ accessToken, refreshToken }).pipe(
        switchMap(res => {
          isRefreshing = false;
          refreshSubject.next(res.accessToken);
          return next(req.clone({
            setHeaders: { Authorization: `Bearer ${res.accessToken}` }
          }));
        }),
        catchError(err => {
          isRefreshing = false;
          authService.logout();
          return throwError(() => err);
        })
      );
    })
  );
};
