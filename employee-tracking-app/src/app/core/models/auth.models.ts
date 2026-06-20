export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  email: string;
  fullName: string;
  roles: string[];
  accessTokenExpiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  roles: string[];
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface RevokeTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface CurrentUser {
  userId: string;
  email: string;
  fullName: string;
  employeeId: string;
  roles: string[];
}

export interface AuthState {
  user: CurrentUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  error: string | null;
}

export interface DecodedToken {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
  employeeId: string;
  exp: number;
  iss: string;
  aud: string;
}

export function decodeToken(token: string): DecodedToken {
  const payload = token.split('.')[1];
  return JSON.parse(atob(payload));
}

export function mapDecodedToUser(decoded: DecodedToken): CurrentUser {
  const rolesClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const roles = Array.isArray(rolesClaim) ? rolesClaim : rolesClaim ? [rolesClaim] : [];
  return {
    userId: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
    email: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
    fullName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
    employeeId: decoded.employeeId,
    roles
  };
}
