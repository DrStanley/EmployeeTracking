import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    loadComponent: () => import('./shared/components/shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'time-tracking',
        loadComponent: () => import('./features/time-tracking/time-tracking.component').then(m => m.TimeTrackingComponent),
        canActivate: [roleGuard(['Employee', 'Manager'])]
      },
      {
        path: 'timesheets',
        loadComponent: () => import('./features/timesheets/timesheets.component').then(m => m.TimesheetsComponent),
        canActivate: [roleGuard(['Employee', 'Manager'])]
      },
      {
        path: 'pto',
        loadComponent: () => import('./features/pto/pto.component').then(m => m.PtoComponent),
        canActivate: [roleGuard(['Employee', 'Manager'])]
      },
      {
        path: 'approvals',
        loadComponent: () => import('./features/approvals/approvals.component').then(m => m.ApprovalsComponent),
        canActivate: [roleGuard(['Manager', 'Admin'])]
      },
      {
        path: 'payroll',
        loadComponent: () => import('./features/payroll/payroll.component').then(m => m.PayrollComponent),
        canActivate: [roleGuard(['Manager', 'Admin'])]
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent)
      },
      {
        path: 'admin',
        canActivate: [roleGuard(['Admin'])],
        children: [
          { path: '', redirectTo: 'policies', pathMatch: 'full' },
          {
            path: 'policies',
            loadComponent: () => import('./features/admin/policies/policies.component').then(m => m.PoliciesComponent)
          },
          {
            path: 'shifts',
            loadComponent: () => import('./features/admin/shifts/shifts.component').then(m => m.ShiftsComponent)
          },
          {
            path: 'holidays',
            loadComponent: () => import('./features/admin/holidays/holidays.component').then(m => m.HolidaysComponent)
          },
          {
            path: 'overtime-rules',
            loadComponent: () => import('./features/admin/overtime-rules/overtime-rules.component').then(m => m.OvertimeRulesComponent)
          },
          {
            path: 'pay-periods',
            loadComponent: () => import('./features/admin/pay-periods/pay-periods.component').then(m => m.PayPeriodsComponent)
          }
        ]
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
