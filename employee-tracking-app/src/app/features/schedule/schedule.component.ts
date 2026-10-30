import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkspaceService } from '../../core/services/workspace.service';
import { Schedule } from '../../core/models/workspace.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, LoadingSkeletonComponent, EmptyStateComponent],
  templateUrl: './schedule.component.html',
  styleUrl: './schedule.component.scss'
})
export class ScheduleComponent implements OnInit {
  private readonly workspace = inject(WorkspaceService);
  loading = signal(true);
  schedule = signal<Schedule | null>(null);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.workspace.schedule().subscribe({
      next: schedule => {
        this.schedule.set(schedule);
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? err.error?.Detail ?? 'Could not load your schedule.');
        this.loading.set(false);
      }
    });
  }
}
