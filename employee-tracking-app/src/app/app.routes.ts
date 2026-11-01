import { Routes } from '@angular/router';
import { activeAccountGuard, authGuard, suspendedAccountGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'account-suspended',
    canActivate: [authGuard, suspendedAccountGuard],
    loadComponent: () => import('./features/auth/suspended/suspended.component').then(m => m.SuspendedComponent)
  },
  {
    path: '',
    loadComponent: () => import('./shared/components/shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard, activeAccountGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'activity',
        loadComponent: () => import('./features/activity/activity.component').then(m => m.ActivityComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'schedule',
        loadComponent: () => import('./features/schedule/schedule.component').then(m => m.ScheduleComponent),
        canActivate: [roleGuard(['Employee', 'Manager'])]
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/calendar/calendar.component').then(m => m.CalendarComponent)
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
        path: 'screen-reviews',
        loadComponent: () => import('./features/screen-reviews/screen-reviews.component').then(m => m.ScreenReviewsComponent),
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
          { path: '', redirectTo: 'people', pathMatch: 'full' },
          {
            path: 'people',
            loadComponent: () => import('./features/admin/people/people.component').then(m => m.PeopleComponent)
          },
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
          },
          {
            path: 'audit',
            loadComponent: () => import('./features/admin/audit/audit.component').then(m => m.AuditComponent)
          }
        ]
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
