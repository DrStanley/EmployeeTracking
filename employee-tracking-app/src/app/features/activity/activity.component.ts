import { Component, inject, OnInit, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AuthStore } from '../../core/auth/auth.store';
import { ActivityService } from '../../core/services/activity.service';
import { TimeEntryService } from '../../core/services/time-entry.service';
import {
  ActivityRangePreset,
  AppActivityReport,
  UserActivitySummary,
  activityRange,
  formatHours
} from '../../core/models/activity.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './activity.component.html',
  styleUrl: './activity.component.scss'
})
export class ActivityComponent implements OnInit {
  private readonly activityService = inject(ActivityService);
  private readonly timeEntry = inject(TimeEntryService);
  readonly store = inject(AuthStore);
  readonly formatHours = formatHours;

  readonly presets: { id: ActivityRangePreset; label: string }[] = [
    { id: 'today', label: 'Today' },
    { id: 'week', label: 'This week' },
    { id: 'month', label: 'This month' }
  ];

  range = signal<ActivityRangePreset>('week');
  selectedEmployeeId = signal<string | null>(null);
  report = signal<AppActivityReport | null>(null);
  workedHours = signal(0);
  users = signal<UserActivitySummary[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);

  logAppName = '';
  logDate = this.todayInput();
  logHours = 1;

  ngOnInit(): void {
    this.reload();
  }

  canViewOthers(): boolean {
    return this.store.isManager() || this.store.isAdmin();
  }

  viewingSelf(): boolean {
    const selected = this.selectedEmployeeId();
    const me = this.store.user()?.employeeId ?? '';
    return !selected || selected === me;
  }

  setRange(preset: ActivityRangePreset): void {
    if (this.range() === preset) return;
    this.range.set(preset);
    this.reload();
  }

  onEmployeeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedEmployeeId.set(value || null);
    this.loadReport();
  }

  isSelected(employeeId: string): boolean {
    const selected = this.selectedEmployeeId();
    if (selected) return selected === employeeId;
    return employeeId === this.store.user()?.employeeId;
  }

  selectUser(employeeId: string): void {
    this.selectedEmployeeId.set(employeeId);
    this.loadReport();
  }

  logTime(event: Event): void {
    event.preventDefault();
    if (this.saving()) return;

    const name = this.logAppName.trim();
    const hours = Number(this.logHours);
    if (!name) {
      this.errorMsg.set('Enter the app name.');
      return;
    }
    if (!hours || hours <= 0 || hours > 24) {
      this.errorMsg.set('Hours must be between 0.25 and 24.');
      return;
    }

    const [year, month, day] = this.logDate.split('-').map(Number);
    const started = new Date(year, (month ?? 1) - 1, day, 9, 0, 0, 0);
    if (Number.isNaN(started.getTime())) {
      this.errorMsg.set('Choose a date.');
      return;
    }
    const ended = new Date(started.getTime() + hours * 3600 * 1000);
    if (ended.getTime() > Date.now() + 5 * 60 * 1000) {
      this.errorMsg.set('That block of time is still in the future.');
      return;
    }

    this.saving.set(true);
    this.errorMsg.set(null);
    this.activityService.record({
      appName: name,
      startedAt: started.toISOString(),
      endedAt: ended.toISOString(),
      source: 'Manual'
    }).subscribe({
      next: () => {
        this.logAppName = '';
        this.saving.set(false);
        this.reload();
      },
      error: err => {
        this.saving.set(false);
        this.errorMsg.set(this.errorDetail(err, 'Could not save that app time.'));
      }
    });
  }

  private reload(): void {
    if (this.canViewOthers()) this.loadUsers();
    this.loadReport();
  }

  private loadUsers(): void {
    this.activityService.getUsers(activityRange(this.range())).subscribe({
      next: users => this.users.set(users),
      error: () => this.users.set([])
    });
  }

  comparisonLabel(): string {
    const gap = this.workedHours() - (this.report()?.totalHours ?? 0);
    const size = formatHours(Math.abs(gap));
    if (gap > 0.05) return `${size} more on the clock`;
    if (gap < -0.05) return `${size} more in apps`;
    return 'Clock time and app time match';
  }

  exportCsv(): void {
    const report = this.report();
    if (!report) return;
    const rows = [
      ['App', 'Hours', 'Sessions', 'Share'],
      ...report.apps.map(app => [app.appName, app.hours, app.sessionCount, `${app.percent}%`]),
      [],
      ['Clocked hours', this.workedHours()],
      ['App hours', report.totalHours]
    ];
    const csv = rows.map(row => row.map(cell => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `activity-${report.employeeName}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  private loadReport(): void {
    const range = activityRange(this.range());
    this.loading.set(true);
    this.errorMsg.set(null);
    forkJoin({
      report: this.activityService.getReport(range, this.selectedEmployeeId()),
      worked: this.timeEntry.getWorked(range.from, range.to, this.selectedEmployeeId()).pipe(
        catchError(() => of({ hours: 0 }))
      )
    }).subscribe({
      next: result => {
        this.report.set(result.report);
        this.workedHours.set(result.worked.hours ?? 0);
        this.loading.set(false);
      },
      error: err => {
        this.report.set(null);
        this.loading.set(false);
        this.errorMsg.set(this.errorDetail(err, 'Could not load activity.'));
      }
    });
  }

  private errorDetail(err: { error?: { detail?: string; Detail?: string } }, fallback: string): string {
    return err?.error?.detail ?? err?.error?.Detail ?? fallback;
  }

  private todayInput(): string {
    const now = new Date();
    const month = `${now.getMonth() + 1}`.padStart(2, '0');
    const day = `${now.getDate()}`.padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
  }
}
