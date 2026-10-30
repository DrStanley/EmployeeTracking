import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { WorkspaceService } from '../../core/services/workspace.service';
import { EmployeeProfile } from '../../core/models/workspace.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { formatHours } from '../../core/models/activity.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatIconModule, LoadingSkeletonComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly workspace = inject(WorkspaceService);
  readonly formatHours = formatHours;

  loading = signal(true);
  profile = signal<EmployeeProfile | null>(null);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.workspace.profile().subscribe({
      next: profile => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? err.error?.Detail ?? 'Could not load your profile.');
        this.loading.set(false);
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
}
