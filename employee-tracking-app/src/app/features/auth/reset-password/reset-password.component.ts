import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './reset-password.component.html',
  styleUrl: '../login/login.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  email = '';
  token = '';
  saving = signal(false);
  done = signal(false);
  errorMsg = signal<string | null>(null);

  form = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirm: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.email || !this.token)
      this.errorMsg.set('This reset link is missing an email or token.');
  }

  submit(): void {
    const { password, confirm } = this.form.getRawValue();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (password !== confirm) {
      this.errorMsg.set('The passwords do not match.');
      return;
    }
    this.saving.set(true);
    this.errorMsg.set(null);
    this.auth.resetPassword(this.email, this.token, password!).subscribe({
      next: () => {
        this.saving.set(false);
        this.done.set(true);
      },
      error: err => {
        this.saving.set(false);
        this.errorMsg.set(err.error?.detail ?? err.error?.Detail ?? 'This reset link is invalid or has expired.');
      }
    });
  }
}
