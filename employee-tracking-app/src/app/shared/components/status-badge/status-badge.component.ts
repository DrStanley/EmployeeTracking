import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimesheetStatus } from '../../../core/models/timesheet.models';
import { PTORequestStatus } from '../../../core/models/pto.models';

type StatusValue = TimesheetStatus | PTORequestStatus | string | number;

interface BadgeConfig { label: string; cssClass: string; icon: string; }

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="badge" [ngClass]="config.cssClass">
      <span class="material-icons icon">{{ config.icon }}</span>
      {{ config.label }}
    </span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 3px 10px;
      border-radius: 9999px;
      font-size: 12px;
      font-weight: 600;
      white-space: nowrap;
    }
    .icon { font-size: 12px !important; }
    .badge-gray   { background: #f1f5f9; color: #475569; }
    .badge-blue   { background: #dbeafe; color: #1e40af; }
    .badge-green  { background: #dcfce7; color: #15803d; }
    .badge-red    { background: #fee2e2; color: #dc2626; }
    .badge-dark   { background: #1e293b; color: #f1f5f9; }
    .badge-yellow { background: #fef9c3; color: #ca8a04; }
  `]
})
export class StatusBadgeComponent {
  @Input() status: StatusValue = '';
  @Input() type: 'timesheet' | 'pto' | 'general' = 'general';

  get config(): BadgeConfig {
    const s = typeof this.status === 'string' ? this.status.toLowerCase() : this.status;

    // Timesheet statuses
    if (s === TimesheetStatus.Draft     || s === 'draft')     return { label: 'Draft',     cssClass: 'badge-gray',   icon: 'edit_note' };
    if (s === TimesheetStatus.Submitted || s === 'submitted') return { label: 'Submitted',  cssClass: 'badge-blue',   icon: 'schedule_send' };
    if (s === TimesheetStatus.Approved  || s === 'approved')  return { label: 'Approved',   cssClass: 'badge-green',  icon: 'check_circle' };
    if (s === TimesheetStatus.Rejected  || s === 'rejected')  return { label: 'Rejected',   cssClass: 'badge-red',    icon: 'cancel' };
    if (s === TimesheetStatus.Locked    || s === 'locked')    return { label: 'Locked',     cssClass: 'badge-dark',   icon: 'lock' };

    // PTO statuses
    if (s === PTORequestStatus.Pending   || s === 'pending')   return { label: 'Pending',   cssClass: 'badge-yellow', icon: 'pending' };
    if (s === PTORequestStatus.Cancelled || s === 'cancelled') return { label: 'Cancelled', cssClass: 'badge-gray',   icon: 'remove_circle' };

    return { label: String(this.status), cssClass: 'badge-gray', icon: 'info' };
  }
}
