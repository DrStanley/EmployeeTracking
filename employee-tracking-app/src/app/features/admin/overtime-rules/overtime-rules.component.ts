import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { OvertimeRule, AttendancePolicy } from '../../../core/models/admin.models';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-overtime-rules',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './overtime-rules.component.html',
  styleUrl: './overtime-rules.component.scss'
})
export class OvertimeRulesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminService);

  loading = signal(true);
  rules = signal<OvertimeRule[]>([]);
  policies = signal<AttendancePolicy[]>([]);
  expandedId = signal<string | null>(null);
  showForm = signal(false);

  form = this.fb.nonNullable.group({
    attendancePolicyId: ['', Validators.required],
    dailyThresholdHours: [8, Validators.required],
    weeklyThresholdHours: [40, Validators.required],
    premiumMultiplier: [1.5, Validators.required],
    hasDoubleTime: [false],
    doubleTimeDailyThreshold: [12, Validators.required]
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.adminService.getOvertimeRules().subscribe({
      next: r => { this.rules.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.adminService.getPolicies().subscribe(p => this.policies.set(p));
  }

  toggleExpand(id: string): void {
    this.expandedId.set(this.expandedId() === id ? null : id);
  }

  openCreate(): void {
    this.form.reset({
      dailyThresholdHours: 8, weeklyThresholdHours: 40,
      premiumMultiplier: 1.5, hasDoubleTime: false, doubleTimeDailyThreshold: 12
    });
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.adminService.createOvertimeRule(this.form.getRawValue()).subscribe({
      next: () => { this.showForm.set(false); this.load(); },
      error: err => alert(err.error?.detail ?? 'Failed to create rule.')
    });
  }

  updateRule(rule: OvertimeRule, field: string, value: any): void {
    this.adminService.updateOvertimeRule(rule.id, { ...rule, [field]: value }).subscribe(() => this.load());
  }

  remove(rule: OvertimeRule): void {
    if (!confirm('Delete this overtime rule?')) return;
    this.adminService.deleteOvertimeRule(rule.id).subscribe(() => this.load());
  }
}
