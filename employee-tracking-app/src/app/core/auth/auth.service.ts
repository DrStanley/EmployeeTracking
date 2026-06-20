import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth.store';
import {
  AuthResponse, LoginRequest, RegisterRequest,
  RefreshTokenRequest, RevokeTokenRequest
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http   = inject(HttpClient);
  private readonly store  = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly base   = `${environment.apiUrl}/auth`;

  login(req: LoginRequest): Observable<AuthResponse> {
    this.store.setLoading(true);
    return this.http.post<AuthResponse>(`${this.base}/login`, req).pipe(
      tap({
        next: (res) => {
          this.store.setTokens(res.accessToken, res.refreshToken);
          this.store.setLoading(false);
        },
        error: () => this.store.setLoading(false)
      })
    );
  }

  register(req: RegisterRequest): Observable<AuthResponse> {
    this.store.setLoading(true);
    return this.http.post<AuthResponse>(`${this.base}/register`, req).pipe(
      tap({
        next: (res) => {
          this.store.setTokens(res.accessToken, res.refreshToken);
          this.store.setLoading(false);
        },
        error: () => this.store.setLoading(false)
      })
    );
  }

  refresh(req: RefreshTokenRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/refresh`, req).pipe(
      tap(res => {
        this.store.setTokens(res.accessToken, res.refreshToken);
      })
    );
  }

  logout(): void {
    const accessToken  = this.store.accessToken();
    const refreshToken = this.store.refreshToken();
    if (accessToken && refreshToken) {
      const req: RevokeTokenRequest = { accessToken, refreshToken };
      this.http.post(`${this.base}/logout`, req).subscribe();
    }
    this.store.clearAuth();
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null  { return this.store.accessToken(); }
  getRefreshToken(): string | null { return this.store.refreshToken(); }
  isAuthenticated(): boolean       { return this.store.isAuthenticated(); }
  hasRole(role: string): boolean   { return this.store.user()?.roles.includes(role) ?? false; }
  hasAnyRole(roles: string[]): boolean {
    return roles.some(r => this.hasRole(r));
  }

  restoreSession(): void {
    this.store.restoreSession();
  }
}
