import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';
import { PersonAccount } from '../../../core/models/admin.models';
import { apiErrorMessage } from '../../../core/http/api-error';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './people.component.html',
  styleUrl: './people.component.scss'
})
export class PeopleComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly admin = inject(AdminService);

  loading = signal(true);
  people = signal<PersonAccount[]>([]);
  showForm = signal(false);
  savingId = signal<string | null>(null);
  errorMsg = signal<string | null>(null);
  formError = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    employeeNumber: ['', Validators.required],
    jobTitle: ['', Validators.required],
    role: this.fb.nonNullable.control<'Employee' | 'Manager'>('Employee', Validators.required)
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set(null);
    this.admin.getPeople().subscribe({
      next: rows => { this.people.set(rows); this.loading.set(false); },
      error: err => { this.errorMsg.set(apiErrorMessage(err, 'Could not load people.')); this.loading.set(false); }
    });
  }

  openCreate(): void {
    this.form.reset({ role: 'Employee' });
    this.formError.set(null);
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.formError.set(null);
    const v = this.form.getRawValue();
    this.admin.addPerson(v).subscribe({
      next: () => { this.showForm.set(false); this.load(); },
      error: err => this.formError.set(apiErrorMessage(err, 'Could not add this person.'))
    });
  }

  saveStatus(person: PersonAccount, accountStatus: string, select: HTMLSelectElement): void {
    if (person.isAdmin || accountStatus === person.accountStatus) return;
    const name = person.fullName || person.email;
    if (accountStatus === 'Suspended' && !confirm(`Suspend ${name}? They can still sign in, but they cannot use the app.`)) {
      select.value = person.accountStatus;
      return;
    }
    if (accountStatus === 'Deleted' && !confirm(`Delete ${name}? They cannot sign in, and this email cannot be registered again.`)) {
      select.value = person.accountStatus;
      return;
    }
    this.savingId.set(person.userId);
    this.errorMsg.set(null);
    this.admin.setPersonStatus(person.userId, accountStatus).subscribe({
      next: updated => {
        this.people.update(rows => rows.map(row => row.userId === updated.userId ? updated : row));
        this.savingId.set(null);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not change that status.'));
        this.savingId.set(null);
        this.load();
      }
    });
  }

  saveRole(person: PersonAccount, role: string): void {
    if (person.isAdmin || role === person.role) return;
    this.savingId.set(person.userId);
    this.errorMsg.set(null);
    this.admin.setPersonRole(person.userId, role).subscribe({
      next: updated => {
        this.people.update(rows => rows.map(row => row.userId === updated.userId ? updated : row));
        this.savingId.set(null);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not change that role.'));
        this.savingId.set(null);
        this.load();
      }
    });
  }
}
