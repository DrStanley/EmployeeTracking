import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../core/services/payroll.service';
import { TimesheetService } from '../../core/services/timesheet.service';
import { AuthStore } from '../../core/auth/auth.store';
import { PayrollReport } from '../../core/models/payroll.models';
import { PayPeriod } from '../../core/models/admin.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './payroll.component.html',
  styleUrl: './payroll.component.scss'
})
export class PayrollComponent implements OnInit {
  private readonly payrollService = inject(PayrollService);
  private readonly timesheetService = inject(TimesheetService);
  readonly store = inject(AuthStore);

  payPeriods = signal<PayPeriod[]>([]);
  selectedPeriod = signal<PayPeriod | null>(null);
  loading = signal(false);
  generating = signal(false);
  report = signal<PayrollReport | null>(null);
  errorMsg = signal<string | null>(null);

  maxBarHours = computed(() => {
    const lines = this.report()?.lines ?? [];
    return Math.max(...lines.map(l => l.regularHours + l.overtimeHours), 1);
  });

  ngOnInit(): void {
    this.timesheetService.getPayPeriods().subscribe({
      next: periods => {
        this.payPeriods.set(periods);
        // Auto-select current period
        const today = new Date();
        const current = periods.find(p =>
          new Date(p.startDate) <= today && new Date(p.endDate) >= today
        ) ?? periods[0] ?? null;
        this.selectedPeriod.set(current);
      }
    });
  }

  onPeriodChange(event: Event): void {
    const id = (event.target as HTMLSelectElement).value;
    const period = this.payPeriods().find(p => p.id === id) ?? null;
    this.selectedPeriod.set(period);
    this.report.set(null);
    this.errorMsg.set(null);
  }

  load(): void {
    const period = this.selectedPeriod();
    if (!period) return;
    this.loading.set(true);
    this.errorMsg.set(null);
    this.payrollService.get(period.id).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'No report found. Try generating one.');
        this.report.set(null);
        this.loading.set(false);
      }
    });
  }

  generate(): void {
    const period = this.selectedPeriod();
    if (!period) return;
    this.generating.set(true);
    this.errorMsg.set(null);
    this.payrollService.generate(period.id).subscribe({
      next: r => { this.report.set(r); this.generating.set(false); },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to generate report.');
        this.generating.set(false);
      }
    });
  }

  exportCsv(): void {
    const r = this.report();
    if (!r) return;
    const headers = ['Employee', 'Dept', 'Regular', 'OT', 'PTO', 'Unpaid', 'Total', 'Status'];
    const rows = r.lines.map(l => [
      l.employeeFullName, l.department, l.regularHours, l.overtimeHours,
      l.ptoHours, l.unpaidHours, l.totalPayableHours, l.timesheetStatus
    ]);
    const csv = [headers, ...rows].map(row => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `payroll-${r.payPeriodName}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }
}