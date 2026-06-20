import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { forkJoin } from 'rxjs';
import { AuthStore } from '../../core/auth/auth.store';
import { PTOService } from '../../core/services/pto.service';
import { TimesheetService } from '../../core/services/timesheet.service';
import { NotificationService } from '../../core/services/notification.service';
import { PTOBalance } from '../../core/models/pto.models';
import { Timesheet } from '../../core/models/timesheet.models';
import { Notification } from '../../core/models/notification.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, LoadingSkeletonComponent, HasRoleDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  readonly store = inject(AuthStore);
  private readonly ptoService = inject(PTOService);
  private readonly timesheetService = inject(TimesheetService);
  private readonly notificationService = inject(NotificationService);

  loading = signal(true);
  ptoBalance = signal<PTOBalance | null>(null);
  pendingApprovals = signal<Timesheet[]>([]);
  recentNotifications = signal<Notification[]>([]);

  today = new Date();

  greeting = computed(() => {
    const hour = this.today.getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  });

  weeklyHours = signal<Record<string, number>>({ mon: 8, tue: 8, wed: 8, thu: 8, fri: 6, sat: 0, sun: 0 });

  ngOnInit(): void {
    const calls: any = {
      notifications: this.notificationService.getAll(true)
    };

    if (this.store.isEmployee() || this.store.isManager()) {
      calls.balance = this.ptoService.getBalance();
    }
    if (this.store.isManager() || this.store.isAdmin()) {
      calls.pending = this.timesheetService.getPending();
    }

    forkJoin(calls).subscribe({
      next: (res: any) => {
        if (res.balance) this.ptoBalance.set(res.balance);
        if (res.pending) this.pendingApprovals.set(res.pending);
        this.recentNotifications.set((res.notifications ?? []).slice(0, 5));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  weekTotal(): number {
    const w = this.weeklyHours();
    return Object.values(w).reduce((a, b) => a + b, 0);
  }
}
