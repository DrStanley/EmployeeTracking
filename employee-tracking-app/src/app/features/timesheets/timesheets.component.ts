import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { TimesheetService } from '../../core/services/timesheet.service';
import { AuthStore } from '../../core/auth/auth.store';
import { Timesheet, TimesheetStatus } from '../../core/models/timesheet.models';
import { PayPeriod } from '../../core/models/admin.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-timesheets',
  standalone: true,
  imports: [
    CommonModule, MatIconModule, StatusBadgeComponent,
    EmptyStateComponent, LoadingSkeletonComponent
  ],
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
  payPeriods = signal<PayPeriod[]>([]);
  selectedPeriod = signal<PayPeriod | null>(null);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.loadPayPeriods();
  }

  loadPayPeriods(): void {
    this.loading.set(true);
    this.timesheetService.getPayPeriods().subscribe({ 
      next: periods => {
        this.payPeriods.set(periods);

        // Auto-select the current pay period
        const today = new Date();
        const current = periods.find(p =>
          new Date(p.startDate) <= today && new Date(p.endDate) >= today
        );

        // Fall back to the most recent period if no current one found
        const selected = current ?? periods[0] ?? null;
        this.selectedPeriod.set(selected);

        if (selected) {
          this.loadTimesheet(selected.id);
        } else {
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  loadTimesheet(payPeriodId: string): void {
    this.loading.set(true);
    this.timesheet.set(null);
    this.errorMsg.set(null);

    const employeeId = this.store.user()?.employeeId ?? '';

    // Just GET — the API auto-creates and updates it on clock-out
    this.timesheetService.get(employeeId, payPeriodId).subscribe({
      next: ts => { this.timesheet.set(ts); this.loading.set(false); },
      error: () => { this.timesheet.set(null); this.loading.set(false); }
    });
  }

  onPeriodChange(event: Event): void {
    const id = (event.target as HTMLSelectElement).value;
    const period = this.payPeriods().find(p => p.id === id) ?? null;
    this.selectedPeriod.set(period);
    if (period) this.loadTimesheet(period.id);
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
        next: () => this.loadTimesheet(this.selectedPeriod()!.id),
        error: err => this.errorMsg.set(err.error?.detail ?? 'Failed to submit.')
      });
    });
  }
}