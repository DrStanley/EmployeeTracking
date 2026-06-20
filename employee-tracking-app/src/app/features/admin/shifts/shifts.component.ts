import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { Shift } from '../../../core/models/admin.models';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-shifts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './shifts.component.html',
  styleUrl: './shifts.component.scss'
})
export class ShiftsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminService);

  loading = signal(true);
  shifts = signal<Shift[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    plannedStart: ['09:00', Validators.required],
    plannedEnd: ['17:00', Validators.required],
    gracePeriodMinutes: [5, Validators.required]
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.adminService.getShifts().subscribe({
      next: s => { this.shifts.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ plannedStart: '09:00', plannedEnd: '17:00', gracePeriodMinutes: 5 });
    this.showForm.set(true);
  }

  openEdit(s: Shift): void {
    this.editingId.set(s.id);
    this.form.patchValue(s);
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    const id = this.editingId();
    const obs = id ? this.adminService.updateShift(id, v) : this.adminService.createShift(v);
    obs.subscribe(() => { this.showForm.set(false); this.load(); });
  }

  toggleActive(s: Shift): void {
    this.adminService.updateShift(s.id, { ...s, isActive: !s.isActive }).subscribe(() => this.load());
  }

  remove(s: Shift): void {
    if (!confirm(`Deactivate shift "${s.name}"?`)) return;
    this.adminService.deleteShift(s.id).subscribe(() => this.load());
  }
}
