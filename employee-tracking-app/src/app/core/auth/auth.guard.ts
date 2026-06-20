import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = () => {
  const store  = inject(AuthStore);
  const router = inject(Router);
  if (store.isAuthenticated()) return true;
  // Check if we have tokens in storage but store not yet hydrated
  const access  = sessionStorage.getItem('accessToken');
  const refresh = localStorage.getItem('refreshToken');
  if (access || refresh) {
    store.restoreSession();
    if (store.isAuthenticated() || refresh) return true;
  }
  return router.createUrlTree(['/login']);
};
