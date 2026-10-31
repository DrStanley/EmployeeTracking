import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { ScreenService } from '../../../core/services/screen.service';
import { apiErrorMessage } from '../../../core/http/api-error';
import { AttendancePolicy } from '../../../core/models/admin.models';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-policies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './policies.component.html',
  styleUrl: './policies.component.scss'
})
export class PoliciesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminService);
  private readonly screenApi = inject(ScreenService);

  loading = signal(true);
  policies = signal<AttendancePolicy[]>([]);
  showForm = signal(false);
  captureEnabled = signal(true);
  captureInterval = signal(5);
  captureMsg = signal<string | null>(null);
  editingId = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    dailyOvertimeThresholdHours: [8, Validators.required],
    weeklyOvertimeThresholdHours: [40, Validators.required],
    overtimeMultiplier: [1.5, Validators.required],
    hasPaidBreaks: [false],
    paidBreakMinutes: [0],
    unpaidBreakMinutes: [30],
    ptoAccrualRatePerPayPeriod: [4, Validators.required],
    maxPTOBalanceHours: [240, Validators.required]
  });

  ngOnInit(): void {
    this.load();
    this.screenApi.config().subscribe({
      next: config => {
        this.captureEnabled.set(config.enabled);
        this.captureInterval.set(config.intervalMinutes);
      }
    });
  }

  saveCapture(): void {
    const intervalMinutes = Math.min(60, Math.max(1, Number(this.captureInterval()) || 1));
    this.captureInterval.set(intervalMinutes);
    this.screenApi.updateConfig({ enabled: this.captureEnabled(), intervalMinutes }).subscribe({
      next: config => {
        this.captureEnabled.set(config.enabled);
        this.captureInterval.set(config.intervalMinutes);
        this.captureMsg.set('Screen capture settings saved.');
      },
      error: err => this.captureMsg.set(apiErrorMessage(err, 'Could not save screen capture settings.'))
    });
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getPolicies().subscribe({
      next: p => { this.policies.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({
      dailyOvertimeThresholdHours: 8, weeklyOvertimeThresholdHours: 40,
      overtimeMultiplier: 1.5, hasPaidBreaks: false, paidBreakMinutes: 0,
      unpaidBreakMinutes: 30, ptoAccrualRatePerPayPeriod: 4, maxPTOBalanceHours: 240
    });
    this.showForm.set(true);
  }

  openEdit(p: AttendancePolicy): void {
    this.editingId.set(p.id);
    this.form.patchValue(p);
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    const id = this.editingId();

    const obs = id ? this.adminService.updatePolicy(id, v) : this.adminService.createPolicy(v);
    obs.subscribe(() => {
      this.showForm.set(false);
      this.load();
    });
  }

  remove(p: AttendancePolicy): void {
    if (!confirm(`Delete policy "${p.name}"? This cannot be undone.`)) return;
    this.adminService.deletePolicy(p.id).subscribe({
      next: () => this.load(),
      error: err => alert(apiErrorMessage(err, 'Failed to delete policy.'))
    });
  }
}
