import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { apiErrorMessage } from './api-error';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
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
