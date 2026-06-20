import { Component, inject, signal } from '@angular/core';
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
  imports: [CommonModule, RouterModule, NotificationBellComponent, HasRoleDirective],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.scss']
})
export class ShellComponent {
  readonly store   = inject(AuthStore);
  readonly auth    = inject(AuthService);
  readonly loading = inject(LoadingService);
  readonly router  = inject(Router);
  readonly dialog  = inject(MatDialog);

  sidebarOpen = signal(true);
  userMenuOpen = signal(false);
  currentPath  = signal('');
  darkMode     = signal(localStorage.getItem('theme') === 'dark');

  readonly navItems: NavItem[] = [
    { label: 'Dashboard',    icon: 'grid_view',      route: '/dashboard' },
    { label: 'Time Tracking', icon: 'schedule',      route: '/time-tracking',  roles: ['Employee', 'Manager'] },
    { label: 'Timesheets',   icon: 'calendar_month', route: '/timesheets',     roles: ['Employee', 'Manager'] },
    { label: 'PTO',          icon: 'beach_access',   route: '/pto',            roles: ['Employee', 'Manager'] },
    { label: 'Approvals',    icon: 'task_alt',       route: '/approvals',      roles: ['Manager', 'Admin'] },
    { label: 'Payroll',      icon: 'bar_chart',      route: '/payroll',        roles: ['Manager', 'Admin'] },
    { label: 'Notifications', icon: 'notifications', route: '/notifications' },
    { label: 'Admin',        icon: 'admin_panel_settings', route: '/admin',   roles: ['Admin'] },
  ];

  constructor() {
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd)
    ).subscribe((e: NavigationEnd) => {
      this.currentPath.set(e.urlAfterRedirects);
    });
    this.applyTheme();
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

  toggleSidebar(): void { this.sidebarOpen.update(v => !v); }
  toggleUserMenu(): void { this.userMenuOpen.update(v => !v); }

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
