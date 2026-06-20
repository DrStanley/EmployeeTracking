import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { TimesheetService } from '../../core/services/timesheet.service';
import { AuthStore } from '../../core/auth/auth.store';
import { Timesheet, TimesheetStatus } from '../../core/models/timesheet.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-timesheets',
  standalone: true,
  imports: [CommonModule, MatIconModule, StatusBadgeComponent, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './timesheets.component.html',
  styleUrl: './timesheets.component.scss'
})
export class TimesheetsComponent implements OnInit {
  private readonly timesheetService = inject(TimesheetService);
  private readonly dialog = inject(MatDialog);
  readonly store = inject(AuthStore);

  readonly TimesheetStatus = TimesheetStatus;

  loading = signal(true);
  timesheet = signal<Timesheet | null>(null);
  errorMsg = signal<string | null>(null);

  // In a real app, this would come from GET /admin/pay-periods
  currentPayPeriodId = signal<string>('current-period-placeholder');

  ngOnInit(): void {
    this.loadTimesheet();
  }

  loadTimesheet(): void {
    this.loading.set(true);
    const employeeId = this.store.user()?.employeeId ?? '';
    this.timesheetService.get(employeeId, this.currentPayPeriodId()).subscribe({
      next: ts => { this.timesheet.set(ts); this.loading.set(false); },
      error: () => { this.timesheet.set(null); this.loading.set(false); }
    });
  }

  submit(): void {
    const ts = this.timesheet();
    if (!ts) return;

    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Submit Timesheet',
        message: 'Once submitted, your timesheet will be sent to your manager for approval. Continue?',
        confirmLabel: 'Submit'
      }
    });

    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.timesheetService.submit(ts.id).subscribe({
        next: () => this.loadTimesheet(),
        error: err => this.errorMsg.set(err.error?.detail ?? 'Failed to submit.')
      });
    });
  }

  statusLabel(status: TimesheetStatus): string {
    return ['Draft', 'Submitted', 'Approved', 'Rejected', 'Locked'][status] ?? 'Unknown';
  }
}
