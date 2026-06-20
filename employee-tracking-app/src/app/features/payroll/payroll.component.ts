import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../core/services/payroll.service';
import { AuthStore } from '../../core/auth/auth.store';
import { PayrollReport } from '../../core/models/payroll.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './payroll.component.html',
  styleUrl: './payroll.component.scss'
})
export class PayrollComponent {
  private readonly payrollService = inject(PayrollService);
  readonly store = inject(AuthStore);

  payPeriodId = signal('');
  loading = signal(false);
  generating = signal(false);
  report = signal<PayrollReport | null>(null);
  errorMsg = signal<string | null>(null);

  maxBarHours = computed(() => {
    const lines = this.report()?.lines ?? [];
    return Math.max(...lines.map(l => l.regularHours + l.overtimeHours), 1);
  });

  load(): void {
    if (!this.payPeriodId()) return;
    this.loading.set(true);
    this.errorMsg.set(null);
    this.payrollService.get(this.payPeriodId()).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'No report found. Try generating one.');
        this.report.set(null);
        this.loading.set(false);
      }
    });
  }

  generate(): void {
    if (!this.payPeriodId()) return;
    this.generating.set(true);
    this.errorMsg.set(null);
    this.payrollService.generate(this.payPeriodId()).subscribe({
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
