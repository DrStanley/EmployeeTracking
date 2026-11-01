import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { WorkspaceService } from '../../core/services/workspace.service';
import { apiErrorMessage } from '../../core/http/api-error';
import { EmployeeProfile } from '../../core/models/workspace.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { formatHours } from '../../core/models/activity.models';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, LoadingSkeletonComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly workspace = inject(WorkspaceService);
  private readonly store = inject(AuthStore);
  private readonly formBuilder = inject(FormBuilder);
  readonly formatHours = formatHours;

  loading = signal(true);
  saving = signal(false);
  profile = signal<EmployeeProfile | null>(null);
  loadError = signal<string | null>(null);
  errorMsg = signal<string | null>(null);
  notice = signal<string | null>(null);
  today = new Date().toISOString().slice(0, 10);
  readonly genders = [
    { value: 'Unspecified', label: 'Not set' },
    { value: 'Female', label: 'Female' },
    { value: 'Male', label: 'Male' },
    { value: 'Other', label: 'Other' },
    { value: 'PreferNotToSay', label: 'Prefer not to say' }
  ];

  readonly maritalStatuses = [
    { value: 'Unspecified', label: 'Not set' },
    { value: 'Single', label: 'Single' },
    { value: 'Married', label: 'Married' },
    { value: 'Divorced', label: 'Divorced' },
    { value: 'Widowed', label: 'Widowed' },
    { value: 'Separated', label: 'Separated' },
    { value: 'PreferNotToSay', label: 'Prefer not to say' }
  ];

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    gender: ['Unspecified'],
    dateOfBirth: [''],
    maritalStatus: ['Unspecified'],
    phone: ['', Validators.maxLength(30)],
    addressLine: ['', Validators.maxLength(200)],
    city: ['', Validators.maxLength(100)],
    state: ['', Validators.maxLength(100)],
    postalCode: ['', Validators.maxLength(20)],
    country: ['', Validators.maxLength(100)]
  });

  ngOnInit(): void {
    this.workspace.profile().subscribe({
      next: profile => {
        this.profile.set(profile);
        this.fill(profile);
        this.loading.set(false);
      },
      error: err => {
        this.loadError.set(apiErrorMessage(err, 'Could not load your profile.'));
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMsg.set(null);
    this.notice.set(null);
    this.workspace.updateProfile({
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      gender: value.gender,
      dateOfBirth: value.dateOfBirth || null,
      maritalStatus: value.maritalStatus,
      phone: value.phone.trim() || null,
      addressLine: value.addressLine.trim() || null,
      city: value.city.trim() || null,
      state: value.state.trim() || null,
      postalCode: value.postalCode.trim() || null,
      country: value.country.trim() || null
    }).subscribe({
      next: profile => {
        this.profile.set(profile);
        this.fill(profile);
        this.store.setDisplayName(`${profile.firstName} ${profile.lastName}`);
        this.notice.set('Profile saved.');
        this.saving.set(false);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not save your profile.'));
        this.saving.set(false);
      }
    });
  }

  initials(): string {
    return (this.profile()?.fullName ?? '')
      .split(' ')
      .map(part => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  private fill(profile: EmployeeProfile): void {
    this.form.patchValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
      gender: profile.gender || 'Unspecified',
      dateOfBirth: profile.dateOfBirth?.slice(0, 10) ?? '',
      maritalStatus: profile.maritalStatus || 'Unspecified',
      phone: profile.phone ?? '',
      addressLine: profile.addressLine ?? '',
      city: profile.city ?? '',
      state: profile.state ?? '',
      postalCode: profile.postalCode ?? '',
      country: profile.country ?? ''
    });
  }
}
