import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { PTOService } from '../../core/services/pto.service';
import { PTOBalance, PTORequest, PTORequestStatus } from '../../core/models/pto.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-pto',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, StatusBadgeComponent, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './pto.component.html',
  styleUrl: './pto.component.scss'
})
export class PtoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly ptoService = inject(PTOService);
  private readonly dialog = inject(MatDialog);

  readonly PTORequestStatus = PTORequestStatus;

  loading = signal(true);
  submitting = signal(false);
  balance = signal<PTOBalance | null>(null);
  requests = signal<PTORequest[]>([]);
  errorMsg = signal<string | null>(null);

  balancePct = computed(() => {
    const b = this.balance();
    if (!b || b.accruedHours === 0) return 0;
    return Math.round((b.availableHours / b.accruedHours) * 100);
  });

  form = this.fb.group({
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    hoursRequested: [8, [Validators.required, Validators.min(1)]],
    notes: ['']
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.ptoService.getBalance().subscribe(b => this.balance.set(b));
    this.ptoService.getMy().subscribe({
      next: reqs => { this.requests.set(reqs); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.errorMsg.set(null);
    const v = this.form.getRawValue();

    this.ptoService.submit({
      startDate: v.startDate!,
      endDate: v.endDate!,
      hoursRequested: v.hoursRequested!,
      notes: v.notes || undefined
    }).subscribe({
      next: () => {
        this.form.reset({ hoursRequested: 8, notes: '' });
        this.submitting.set(false);
        this.load();
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to submit request.');
        this.submitting.set(false);
      }
    });
  }

  cancel(request: PTORequest): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel PTO Request',
        message: request.status === PTORequestStatus.Approved
          ? 'This request was already approved. Cancelling will restore the hours to your balance. Continue?'
          : 'Are you sure you want to cancel this request?',
        confirmLabel: 'Yes, Cancel',
        confirmColor: 'warn'
      }
    });

    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.ptoService.cancel(request.id).subscribe(() => this.load());
    });
  }
}
