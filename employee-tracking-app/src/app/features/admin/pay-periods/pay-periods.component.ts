import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { AdminService } from '../../../core/services/admin.service';
import { PayPeriod } from '../../../core/models/admin.models';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-pay-periods',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './pay-periods.component.html',
  styleUrl: './pay-periods.component.scss'
})
export class PayPeriodsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminService);
  private readonly dialog = inject(MatDialog);

  loading = signal(true);
  periods = signal<PayPeriod[]>([]);
  showForm = signal(false);

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required]
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.adminService.getPayPeriods().subscribe({
      next: p => { this.periods.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  isCurrent(p: PayPeriod): boolean {
    const now = new Date();
    return new Date(p.startDate) <= now && new Date(p.endDate) >= now;
  }

  openCreate(): void {
    this.form.reset();
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.adminService.createPayPeriod(this.form.getRawValue()).subscribe({
      next: () => { this.showForm.set(false); this.load(); },
      error: err => alert(err.error?.detail ?? 'Failed to create pay period.')
    });
  }

  lock(p: PayPeriod): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Lock Pay Period',
        message: `Locking "${p.name}" prevents further edits. Make sure all timesheets are approved before continuing.`,
        confirmLabel: 'Lock Period',
        confirmColor: 'warn'
      }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.adminService.lockPayPeriod(p.id).subscribe(() => this.load());
    });
  }
}
