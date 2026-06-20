import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 500) {
        snackBar.open(
          error.error?.detail ?? 'An unexpected server error occurred.',
          'Dismiss',
          { duration: 5000, panelClass: ['snack-error'] }
        );
      }
      if (error.status === 403) {
        snackBar.open(
          error.error?.detail ?? 'Access denied. Insufficient permissions.',
          'Dismiss',
          { duration: 4000, panelClass: ['snack-warn'] }
        );
      }
      return throwError(() => error);
    })
  );
};
