import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { NotificationService } from '../../core/services/notification.service';
import { Notification, NotificationType } from '../../core/models/notification.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);

  readonly NotificationType = NotificationType;

  loading = signal(true);
  all = signal<Notification[]>([]);
  filter = signal<'all' | 'unread'>('all');

  filtered = computed(() => {
    const list = this.all();
    return this.filter() === 'unread' ? list.filter(n => !n.isRead) : list;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.notificationService.getAll().subscribe({
      next: list => { this.all.set(list); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  markRead(n: Notification): void {
    if (n.isRead) return;
    this.notificationService.markRead(n.id).subscribe(() => {
      this.all.update(list => list.map(x => x.id === n.id ? { ...x, isRead: true } : x));
    });
  }

  markAllRead(): void {
    this.notificationService.markAllRead().subscribe(() => {
      this.all.update(list => list.map(x => ({ ...x, isRead: true })));
    });
  }

  iconFor(type: NotificationType): { icon: string; cssClass: string } {
    switch (type) {
      case NotificationType.MissedPunch:       return { icon: 'warning', cssClass: 'icon-danger' };
      case NotificationType.PendingApproval:   return { icon: 'info', cssClass: 'icon-info' };
      case NotificationType.TimesheetApproved: return { icon: 'check_circle', cssClass: 'icon-success' };
      case NotificationType.TimesheetRejected: return { icon: 'cancel', cssClass: 'icon-danger' };
      case NotificationType.LowPTOBalance:     return { icon: 'event_busy', cssClass: 'icon-warning' };
      case NotificationType.OvertimeAlert:     return { icon: 'schedule', cssClass: 'icon-warning' };
      default:                                  return { icon: 'notifications', cssClass: 'icon-info' };
    }
  }
}
