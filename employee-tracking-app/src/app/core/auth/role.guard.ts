import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const roleGuard = (allowed: string | string[]): CanActivateFn => () => {
  const store  = inject(AuthStore);
  const router = inject(Router);
  const user   = store.user();
  if (!user) return router.createUrlTree(['/login']);
  const roles = Array.isArray(allowed) ? allowed : [allowed];
  const hasRole = roles.some(r => user.roles.includes(r));
  if (hasRole) return true;
  return router.createUrlTree(['/dashboard']);
};
