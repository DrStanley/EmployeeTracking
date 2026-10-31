import { Component, inject, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { AuthService } from '../../../core/auth/auth.service';
import { LoadingService } from '../../../core/services/loading.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { HasRoleDirective } from '../../directives/has-role.directive';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';
import { MatIconModule } from '@angular/material/icon';
import { ActivityTrackerService } from '../../../core/services/activity-tracker.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
  badge?: number;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, NotificationBellComponent, HasRoleDirective],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.scss']
})
export class ShellComponent implements OnDestroy {
  readonly store   = inject(AuthStore);
  readonly auth    = inject(AuthService);
  readonly loading = inject(LoadingService);
  readonly router  = inject(Router);
  readonly dialog  = inject(MatDialog);
  private readonly activityTracker = inject(ActivityTrackerService);
  isCollapsed = signal(false);
  sidebarOpen = signal(true);
  userMenuOpen = signal(false);
  mobileMoreOpen = signal(false);
  currentPath  = signal('');
  darkMode     = signal(localStorage.getItem('theme') === 'dark');
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'grid_view', route: '/dashboard' },
    { label: 'Activity', icon: 'apps', route: '/activity' },
    { label: 'Time Tracking', icon: 'schedule', route: '/time-tracking', roles: ['Employee', 'Manager'] },
    { label: 'Schedule', icon: 'event_note', route: '/schedule', roles: ['Employee', 'Manager'] },
    { label: 'Calendar', icon: 'calendar_month', route: '/calendar' },
    { label: 'Timesheets', icon: 'fact_check', route: '/timesheets', roles: ['Employee', 'Manager'] },
    { label: 'PTO', icon: 'beach_access', route: '/pto', roles: ['Employee', 'Manager'] },
    { label: 'Approvals', icon: 'task_alt', route: '/approvals', roles: ['Manager', 'Admin'] },
    { label: 'Payroll', icon: 'bar_chart', route: '/payroll', roles: ['Manager', 'Admin'] },
    { label: 'Screen Reviews', icon: 'monitor', route: '/screen-reviews', roles: ['Manager', 'Admin'] },
    { label: 'Notifications', icon: 'notifications', route: '/notifications' },
  ];

  readonly adminNavItems: NavItem[] = [
    { label: 'Policies', icon: 'rule', route: '/admin/policies' },
    { label: 'Shifts', icon: 'work_history', route: '/admin/shifts' },
    { label: 'Holidays', icon: 'celebration', route: '/admin/holidays' },
    { label: 'Overtime Rules', icon: 'bolt', route: '/admin/overtime-rules' },
    { label: 'Pay Periods', icon: 'date_range', route: '/admin/pay-periods' },
    { label: 'Audit History', icon: 'history', route: '/admin/audit' },
  ];
  constructor() {
    this.currentPath.set(this.router.url);
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd)
    ).subscribe((e: NavigationEnd) => {
      this.currentPath.set(e.urlAfterRedirects);
      this.mobileMoreOpen.set(false);
    });
    this.applyTheme();
    this.activityTracker.start();
  }

  ngOnDestroy(): void {
    this.activityTracker.stop();
  }

  visibleNavItems(): NavItem[] {
    const user = this.store.user();
    if (!user) return [];
    return this.navItems.filter(item => {
      if (!item.roles) return true;
      return item.roles.some(r => user.roles.includes(r));
    });
  }

  isActive(route: string): boolean {
    return this.currentPath().startsWith(route);
  }

  pageTitle(): string {
    const path = this.currentPath().split('?')[0];
    const items = [
      ...this.navItems,
      ...this.adminNavItems,
      { label: 'Profile', icon: 'person', route: '/profile' }
    ];
    const match = items
      .filter(item => path === item.route || path.startsWith(item.route + '/'))
      .sort((a, b) => b.route.length - a.route.length)[0];
    if (match) return match.label;
    const segment = path.split('/').filter(Boolean).pop() ?? 'Home';
    return segment.replace(/-/g, ' ').replace(/\b\w/g, letter => letter.toUpperCase());
  }

  mobilePrimary(): NavItem[] {
    return this.visibleNavItems().slice(0, 4);
  }

  overflowNavItems(): NavItem[] {
    const extra = this.visibleNavItems().slice(4);
    return this.store.isAdmin() ? [...extra, ...this.adminNavItems] : extra;
  }

  overflowActive(): boolean {
    return this.overflowNavItems().some(item => this.isActive(item.route));
  }

  toggleSidebar(): void { this.sidebarOpen.update(v => !v); this.isCollapsed.update(v => !v); }
  toggleUserMenu(): void { this.userMenuOpen.update(v => !v); }
  toggleMobileMore(): void { this.mobileMoreOpen.update(open => !open); }

  toggleDarkMode(): void {
    const dark = !this.darkMode();
    this.darkMode.set(dark);
    localStorage.setItem('theme', dark ? 'dark' : 'light');
    this.applyTheme();
  }

  private applyTheme(): void {
    document.documentElement.setAttribute('data-theme', this.darkMode() ? 'dark' : 'light');
  }

  confirmLogout(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Sign Out',
        message: 'Are you sure you want to sign out? Any unsaved changes will be lost.',
        confirmLabel: 'Sign Out',
        confirmColor: 'warn'
      }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) this.auth.logout();
    });
  }

  getInitials(): string {
    const name = this.store.user()?.fullName ?? '';
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
