import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkspaceService } from '../../../core/services/workspace.service';
import { apiErrorMessage } from '../../../core/http/api-error';
import { AuditLogEntry } from '../../../core/models/workspace.models';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [CommonModule, LoadingSkeletonComponent, EmptyStateComponent],
  templateUrl: './audit.component.html',
  styleUrl: './audit.component.scss'
})
export class AuditComponent implements OnInit {
  private readonly workspace = inject(WorkspaceService);
  loading = signal(true);
  logs = signal<AuditLogEntry[]>([]);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.workspace.audit().subscribe({
      next: logs => {
        this.logs.set(logs);
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not load the audit history.'));
        this.loading.set(false);
      }
    });
  }
}
