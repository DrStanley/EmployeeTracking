import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';

function passwordsMatch(control: AbstractControl) {
  const pw = control.get('password')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw === cpw ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  readonly store = inject(AuthStore);

  showPassword = signal(false);

  form = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
    roles: this.fb.group({
      Employee: [true],
      Manager: [false],
      Admin: [false]
    })
  }, { validators: passwordsMatch });

  passwordStrength = computed(() => {
    const pw = this.form.get('password')?.value ?? '';
    if (pw.length === 0) return { label: '', pct: 0, color: '' };
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;

    if (score <= 1) return { label: 'Weak', pct: 33, color: 'var(--color-danger)' };
    if (score <= 2) return { label: 'Fair', pct: 66, color: 'var(--color-warning)' };
    return { label: 'Strong', pct: 100, color: 'var(--color-success)' };
  });

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const roles = Object.entries(v.roles!)
      .filter(([, checked]) => checked)
      .map(([role]) => role);

    if (roles.length === 0) {
      this.store.setError('Select at least one role.');
      return;
    }

    this.authService.register({
      firstName: v.firstName!,
      lastName: v.lastName!,
      email: v.email!,
      password: v.password!,
      roles
    }).subscribe();
  }
}
