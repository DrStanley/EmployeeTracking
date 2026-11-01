import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { apiErrorMessage } from './api-error';
import { AuthStore } from '../auth/auth.store';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const store = inject(AuthStore);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = apiErrorMessage(error, '');
      if (error.status === 403 && message.toLowerCase().includes('suspended')) {
        store.setAccountStatus('Suspended');
        router.navigate(['/account-suspended']);
        return throwError(() => error);
      }

      const showOnPage = req.url.includes('/auth/');
      if (!showOnPage && error.status >= 400 && error.status !== 401) {
        snackBar.open(
          apiErrorMessage(error, 'Something went wrong.'),
          'Dismiss',
          { duration: 6000, panelClass: [error.status === 403 ? 'snack-warn' : 'snack-error'] }
        );
      }
      return throwError(() => error);
    })
  );
};
