import { computed, inject } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { AuthState, CurrentUser, decodeToken, mapDecodedToUser } from '../models/auth.models';

const AUTH_STATE: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isLoading: false,
  error: null
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(AUTH_STATE),

  withComputed((store) => ({
    isAuthenticated: computed(() => {
      const token = store.accessToken();
      if (!token) return false;
      try {
        const decoded = decodeToken(token);
        return decoded.exp * 1000 > Date.now();
      } catch {
        return false;
      }
    }),
    isEmployee: computed(() => store.user()?.roles.includes('Employee') ?? false),
    isManager:  computed(() => store.user()?.roles.includes('Manager') ?? false),
    isAdmin:    computed(() => store.user()?.roles.includes('Admin') ?? false),
    primaryRole: computed(() => {
      const roles = store.user()?.roles ?? [];
      if (roles.includes('Admin'))   return 'Admin';
      if (roles.includes('Manager')) return 'Manager';
      return 'Employee';
    }),
    firstName: computed(() => {
      const full = store.user()?.fullName ?? '';
      return full.split(' ')[0];
    })
  })),

  withMethods((store) => ({
    setTokens(accessToken: string, refreshToken: string): void {
      try {
        const decoded = decodeToken(accessToken);
        const user: CurrentUser = mapDecodedToUser(decoded);
        sessionStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        patchState(store, { user, accessToken, refreshToken, error: null });
      } catch {
        patchState(store, { error: 'Failed to decode token' });
      }
    },

    setLoading(isLoading: boolean): void {
      patchState(store, { isLoading });
    },

    setError(error: string | null): void {
      patchState(store, { error, isLoading: false });
    },

    updateAccessToken(accessToken: string): void {
      try {
        const decoded = decodeToken(accessToken);
        const user = mapDecodedToUser(decoded);
        sessionStorage.setItem('accessToken', accessToken);
        patchState(store, { accessToken, user });
      } catch { /* ignore */ }
    },

    clearAuth(): void {
      sessionStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      patchState(store, { ...AUTH_STATE });
    },

    restoreSession(): void {
      const accessToken  = sessionStorage.getItem('accessToken');
      const refreshToken = localStorage.getItem('refreshToken');
      if (accessToken && refreshToken) {
        try {
          const decoded = decodeToken(accessToken);
          if (decoded.exp * 1000 > Date.now()) {
            const user = mapDecodedToUser(decoded);
            patchState(store, { user, accessToken, refreshToken });
          } else {
            // Access token expired but we have refresh token
            patchState(store, { accessToken, refreshToken });
          }
        } catch {
          sessionStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      }
    }
  }))
);
