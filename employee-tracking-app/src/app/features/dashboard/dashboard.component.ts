import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthStore } from '../../core/auth/auth.store';
import { PTOService } from '../../core/services/pto.service';
import { TimesheetService } from '../../core/services/timesheet.service';
import { NotificationService } from '../../core/services/notification.service';
import { PTOBalance } from '../../core/models/pto.models';
import { Timesheet } from '../../core/models/timesheet.models';
import { Notification } from '../../core/models/notification.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';
import { ActivityService } from '../../core/services/activity.service';
import { TimeEntryService } from '../../core/services/time-entry.service';
import { AppUsageItem, activityRange, formatHours } from '../../core/models/activity.models';
import { Presence, WeeklyHours } from '../../core/models/workspace.models';

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
  private readonly activityService = inject(ActivityService);
  private readonly timeEntry = inject(TimeEntryService);
  readonly formatHours = formatHours;

  loading = signal(true);
  ptoBalance = signal<PTOBalance | null>(null);
  pendingApprovals = signal<Timesheet[]>([]);
  recentNotifications = signal<Notification[]>([]);
  weekApps = signal<AppUsageItem[]>([]);
  clockStatus = signal<string>('clocked-out');
  clockedInAt = signal<string | null>(null);
  presence = signal<Presence[]>([]);

  today = new Date();

  greeting = computed(() => {
    const hour = this.today.getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  });

  weeklyHours = signal<Record<string, number>>({ mon: 0, tue: 0, wed: 0, thu: 0, fri: 0, sat: 0, sun: 0 });

  ngOnInit(): void {
    const calls: any = {
      notifications: this.notificationService.getAll(true),
      activity: this.activityService.getReport(activityRange('week')).pipe(
        catchError(() => of(null))
      )
    };

    if (this.store.isEmployee() || this.store.isManager()) {
      calls.balance = this.ptoService.getBalance();
      calls.status = this.timeEntry.getClockStatus().pipe(catchError(() => of(null)));
    }
    if (this.store.isEmployee() || this.store.isManager() || this.store.isAdmin()) {
      calls.week = this.timeEntry.getWeek().pipe(catchError(() => of(null)));
    }
    if (this.store.isManager() || this.store.isAdmin()) {
      calls.pending = this.timesheetService.getPending();
      calls.presence = this.timeEntry.getPresence().pipe(catchError(() => of([])));
    }

    forkJoin(calls).subscribe({
      next: (res: any) => {
        if (res.balance) this.ptoBalance.set(res.balance);
        if (res.pending) this.pendingApprovals.set(res.pending);
        this.recentNotifications.set((res.notifications ?? []).slice(0, 5));
        this.weekApps.set((res.activity?.apps ?? []).slice(0, 4));
        if (res.status) {
          this.clockStatus.set(res.status.status);
          this.clockedInAt.set(res.status.clockedInAt ?? null);
        }
        if (res.week) this.applyWeek(res.week);
        if (res.presence) this.presence.set(res.presence);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  weekTotal(): number {
    const w = this.weeklyHours();
    return Object.values(w).reduce((a, b) => a + b, 0);
  }

  statusLabel(): string {
    switch (this.clockStatus()) {
      case 'clocked-in': return 'Clocked In';
      case 'on-break': return 'On Break';
      default: return 'Clocked Out';
    }
  }

  presenceLabel(status: string): string {
    switch (status) {
      case 'clocked-in': return 'Working';
      case 'on-break': return 'On break';
      default: return 'Out';
    }
  }

  private applyWeek(week: WeeklyHours): void {
    this.weeklyHours.set({
      mon: week.mon, tue: week.tue, wed: week.wed, thu: week.thu,
      fri: week.fri, sat: week.sat, sun: week.sun
    });
  }
}
