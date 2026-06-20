# Employee Tracking — Angular Frontend

Modern Angular 18 frontend for the Employee Tracking System ASP.NET Core API.

## Setup

```bash
npm install
npm start
```

The app runs on `http://localhost:4200` and proxies API calls to the URL configured
in `src/environments/environment.ts` (defaults to `https://localhost/api`).

## Build

```bash
npm run build
```

Output goes to `dist/employee-tracking-app`.

## Architecture

- **Standalone components** throughout — no NgModules
- **NgRx Signal Store** for auth state (`core/auth/auth.store.ts`)
- **Functional interceptors** for API base URL, auth token refresh, error handling, and loading state
- **Role-based route guards** (`core/auth/role.guard.ts`)
- **Lazy-loaded feature routes** for fast initial load

## Folder Structure

```
src/app/
├── core/         → auth, http interceptors, services, models (singleton, app-wide)
├── features/     → one folder per route/page
└── shared/       → reusable components, directives, pipes
```

## Default Test Accounts

Create accounts via the Register page. Use the **Admin** role to access
`/admin` configuration pages (policies, shifts, holidays, overtime rules, pay periods).

## Notes

- Access tokens are stored in `sessionStorage`, refresh tokens in `localStorage`
- Tokens auto-refresh on 401 via `tokenRefreshInterceptor`
- Dark mode and additional polish can be layered on top of the existing CSS custom properties in `src/styles/_variables.scss`
