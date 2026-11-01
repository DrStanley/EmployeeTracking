import { computed } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { AuthState, CurrentUser, decodeToken, mapDecodedToUser } from '../models/auth.models';

function displayName(value: string | null | undefined): string | null {
  const name = value?.trim() ?? '';
  if (!name || name.includes('@')) return null;
  return name;
}

const AUTH_STATE: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isLoading: false,
  error: null,
  accountStatus: 'Active'
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
      const full = (store.user()?.fullName ?? '').trim();
      if (!full || full.includes('@')) return '';
      return full.split(' ')[0];
    })
  })),

  withMethods((store) => ({
    setTokens(accessToken: string, refreshToken: string, fullName?: string): void {
      try {
        const decoded = decodeToken(accessToken);
        const user: CurrentUser = mapDecodedToUser(decoded);
        const name = displayName(fullName) ?? displayName(user.fullName);
        if (name) {
          user.fullName = name;
          sessionStorage.setItem('displayName', name);
        }
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

    setAccountStatus(accountStatus: string): void {
      const status = accountStatus === 'Suspended' || accountStatus === 'Deleted' ? accountStatus : 'Active';
      sessionStorage.setItem('accountStatus', status);
      patchState(store, { accountStatus: status });
    },

    updateAccessToken(accessToken: string): void {
      try {
        const decoded = decodeToken(accessToken);
        const user = mapDecodedToUser(decoded);
        const name = displayName(sessionStorage.getItem('displayName')) ?? displayName(user.fullName);
        if (name) user.fullName = name;
        sessionStorage.setItem('accessToken', accessToken);
        patchState(store, { accessToken, user });
      } catch { /* ignore */ }
    },

    clearAuth(): void {
      sessionStorage.removeItem('accessToken');
      sessionStorage.removeItem('displayName');
      sessionStorage.removeItem('accountStatus');
      localStorage.removeItem('refreshToken');
      patchState(store, { ...AUTH_STATE });
    },

    setDisplayName(fullName: string): void {
      const name = displayName(fullName);
      if (!name) return;
      sessionStorage.setItem('displayName', name);
      const user = store.user();
      if (user) patchState(store, { user: { ...user, fullName: name } });
    },

    restoreSession(): void {
      const accessToken  = sessionStorage.getItem('accessToken');
      const refreshToken = localStorage.getItem('refreshToken');
      if (accessToken && refreshToken) {
        try {
          const decoded = decodeToken(accessToken);
          if (decoded.exp * 1000 > Date.now()) {
            const user = mapDecodedToUser(decoded);
            const name = displayName(sessionStorage.getItem('displayName')) ?? displayName(user.fullName);
            if (name) user.fullName = name;
            const saved = sessionStorage.getItem('accountStatus');
            const accountStatus = saved === 'Suspended' || saved === 'Deleted' ? saved : 'Active';
            patchState(store, { user, accessToken, refreshToken, accountStatus });
          } else {
            const saved = sessionStorage.getItem('accountStatus');
            const accountStatus = saved === 'Suspended' || saved === 'Deleted' ? saved : 'Active';
            patchState(store, { accessToken, refreshToken, accountStatus });
          }
        } catch {
          sessionStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      }
    }
  }))
);
