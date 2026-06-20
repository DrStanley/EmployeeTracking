import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TimesheetService } from '../../core/services/timesheet.service';
import { PTOService } from '../../core/services/pto.service';
import { Timesheet } from '../../core/models/timesheet.models';
import { PTORequest } from '../../core/models/pto.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, StatusBadgeComponent, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './approvals.component.html',
  styleUrl: './approvals.component.scss'
})
export class ApprovalsComponent implements OnInit {
  private readonly timesheetService = inject(TimesheetService);
  private readonly ptoService = inject(PTOService);

  activeTab = signal<'timesheets' | 'pto'>('timesheets');
  loading = signal(true);

  timesheets = signal<Timesheet[]>([]);
  ptoRequests = signal<PTORequest[]>([]);

  expandedId = signal<string | null>(null);
  rejectingId = signal<string | null>(null);
  rejectReason = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.timesheetService.getPending().subscribe({
      next: ts => { this.timesheets.set(ts); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.ptoService.getPending().subscribe(reqs => this.ptoRequests.set(reqs));
  }

  toggleExpand(id: string): void {
    this.expandedId.set(this.expandedId() === id ? null : id);
  }

  approveTimesheet(ts: Timesheet): void {
    this.timesheetService.approve(ts.id, {}).subscribe(() => this.load());
  }

  startReject(id: string): void {
    this.rejectingId.set(id);
    this.rejectReason.set('');
  }

  cancelReject(): void {
    this.rejectingId.set(null);
  }

  confirmRejectTimesheet(ts: Timesheet): void {
    if (!this.rejectReason().trim()) return;
    this.timesheetService.reject(ts.id, { reason: this.rejectReason() }).subscribe(() => {
      this.rejectingId.set(null);
      this.load();
    });
  }

  approvePTO(req: PTORequest): void {
    this.ptoService.approve(req.id, {}).subscribe(() => this.load());
  }

  confirmRejectPTO(req: PTORequest): void {
    if (!this.rejectReason().trim()) return;
    this.ptoService.reject(req.id, { reason: this.rejectReason() }).subscribe(() => {
      this.rejectingId.set(null);
      this.load();
    });
  }
}
