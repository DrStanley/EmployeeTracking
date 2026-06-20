import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { Holiday } from '../../../core/models/admin.models';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-holidays',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './holidays.component.html',
  styleUrl: './holidays.component.scss'
})
export class HolidaysComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminService);

  loading = signal(true);
  holidays = signal<Holiday[]>([]);
  year = signal(new Date().getFullYear());
  showForm = signal(false);

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    date: ['', Validators.required],
    isRecurringAnnually: [false]
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.adminService.getHolidays(this.year()).subscribe({
      next: h => { this.holidays.set(h); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  changeYear(delta: number): void {
    this.year.update(y => y + delta);
    this.load();
  }

  openCreate(): void {
    this.form.reset({ isRecurringAnnually: false });
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.adminService.createHoliday(this.form.getRawValue()).subscribe(() => {
      this.showForm.set(false);
      this.load();
    });
  }

  remove(h: Holiday): void {
    if (!confirm(`Remove holiday "${h.name}"?`)) return;
    this.adminService.deleteHoliday(h.id).subscribe(() => this.load());
  }
}
