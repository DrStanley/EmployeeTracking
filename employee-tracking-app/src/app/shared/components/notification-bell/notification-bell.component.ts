import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatBadgeModule } from '@angular/material/badge';
import { interval, Subscription } from 'rxjs';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification, NotificationType } from '../../../core/models/notification.models';
import { TimeAgoPipe } from '../../pipes/duration.pipe';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule, MatBadgeModule, TimeAgoPipe],
  template: `
    <div class="bell-wrap" [class.open]="isOpen()">
      <button class="bell-btn" (click)="toggle()" [attr.aria-label]="'Notifications, ' + unread() + ' unread'">
        <span class="material-icons">notifications</span>
        @if (unread() > 0) {
          <span class="badge">{{ unread() > 99 ? '99+' : unread() }}</span>
        }
      </button>

      @if (isOpen()) {
        <div class="dropdown" (click)="$event.stopPropagation()">
          <div class="dropdown-header">
            <span class="font-semibold">Notifications</span>
            @if (unread() > 0) {
              <button class="btn-link" (click)="markAllRead()">Mark all read</button>
            }
          </div>

          @if (loading()) {
            <div class="loading-state">Loading...</div>
          } @else if (notifications().length === 0) {
            <div class="empty-notifs">
              <span class="material-icons">notifications_none</span>
              <p>No notifications yet</p>
            </div>
          } @else {
            <div class="notif-list">
              @for (n of notifications(); track n.id) {
                <div class="notif-item" [class.unread]="!n.isRead" (click)="markRead(n)">
                  <span class="notif-icon material-icons" [ngClass]="getIconClass(n.type)">
                    {{ getIcon(n.type) }}
                  </span>
                  <div class="notif-body">
                    <p class="notif-msg">{{ n.message }}</p>
                    <span class="notif-time">{{ n.sentAt | timeAgo }}</span>
                  </div>
                  @if (!n.isRead) { <span class="dot"></span> }
                </div>
              }
            </div>
          }

          <div class="dropdown-footer">
            <a routerLink="/notifications" (click)="isOpen.set(false)">View all notifications</a>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .bell-wrap { position: relative; }
    .bell-btn {
      position: relative;
      width: 40px; height: 40px;
      border-radius: 50%;
      border: none;
      background: transparent;
      cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      transition: background var(--transition-fast);
      &:hover { background: var(--color-background); }
      .material-icons { font-size: 22px; color: var(--color-text-secondary); }
    }
    .badge {
      position: absolute;
      top: 4px; right: 4px;
      background: var(--color-danger);
      color: white;
      font-size: 10px;
      font-weight: 700;
      min-width: 18px;
      height: 18px;
      border-radius: 9999px;
      display: flex; align-items: center; justify-content: center;
      padding: 0 4px;
    }
    .dropdown {
      position: absolute;
      top: calc(100% + 8px);
      right: 0;
      width: 360px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-xl);
      z-index: 1000;
      overflow: hidden;
    }
    .dropdown-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px;
      border-bottom: 1px solid var(--color-border);
      font-size: 14px;
    }
    .btn-link {
      background: none; border: none; cursor: pointer;
      color: var(--color-primary); font-size: 13px;
    }
    .notif-list { max-height: 380px; overflow-y: auto; }
    .notif-item {
      display: flex; align-items: flex-start; gap: 12px;
      padding: 12px 16px;
      border-bottom: 1px solid var(--color-border);
      cursor: pointer;
      transition: background var(--transition-fast);
      position: relative;
      &:hover { background: var(--color-background); }
      &.unread { background: var(--color-primary-muted); }
    }
    .notif-icon {
      font-size: 20px !important;
      margin-top: 2px;
      flex-shrink: 0;
    }
    .icon-blue   { color: var(--color-primary); }
    .icon-green  { color: var(--color-success); }
    .icon-red    { color: var(--color-danger); }
    .icon-yellow { color: var(--color-warning); }
    .notif-body { flex: 1; min-width: 0; }
    .notif-msg  { font-size: 13px; color: var(--color-text-primary); line-height: 1.4; margin: 0 0 4px; }
    .notif-time { font-size: 11px; color: var(--color-text-muted); }
    .dot {
      width: 8px; height: 8px;
      background: var(--color-primary);
      border-radius: 50%;
      flex-shrink: 0;
      margin-top: 6px;
    }
    .dropdown-footer {
      padding: 12px 16px;
      text-align: center;
      border-top: 1px solid var(--color-border);
      a { font-size: 13px; color: var(--color-primary); }
    }
    .empty-notifs {
      padding: 32px;
      text-align: center;
      .material-icons { font-size: 40px; color: var(--color-text-muted); }
      p { font-size: 13px; color: var(--color-text-muted); margin: 8px 0 0; }
    }
    .loading-state { padding: 24px; text-align: center; color: var(--color-text-muted); font-size: 13px; }
  `]
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private readonly svc = inject(NotificationService);

  isOpen       = signal(false);
  notifications = signal<Notification[]>([]);
  loading      = signal(false);
  unread       = signal(0);

  private pollSub?: Subscription;
  private clickHandler = () => this.isOpen.set(false);

  ngOnInit(): void {
    this.load();
    this.pollSub = interval(environment.notificationPollInterval).subscribe(() => this.load());
    document.addEventListener('click', this.clickHandler);
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
    document.removeEventListener('click', this.clickHandler);
  }

  toggle(): void { this.isOpen.update(v => !v); if (this.isOpen()) this.load(); }

  private load(): void {
    this.svc.getAll(true).subscribe({
      next: (data) => {
        this.notifications.set(data.slice(0, 8));
        this.unread.set(data.filter(n => !n.isRead).length);
      }
    });
  }

  markRead(n: Notification): void {
    if (!n.isRead) {
      this.svc.markRead(n.id).subscribe(() => this.load());
    }
  }

  markAllRead(): void {
    this.svc.markAllRead().subscribe(() => this.load());
  }

  getIcon(type: NotificationType): string {
    const map: Record<number, string> = {
      [NotificationType.MissedPunch]:      'fingerprint',
      [NotificationType.PendingApproval]:  'pending_actions',
      [NotificationType.TimesheetApproved]:'check_circle',
      [NotificationType.TimesheetRejected]:'cancel',
      [NotificationType.LowPTOBalance]:    'beach_access',
      [NotificationType.OvertimeAlert]:    'warning'
    };
    return map[type] ?? 'notifications';
  }

  getIconClass(type: NotificationType): string {
    if ([NotificationType.TimesheetApproved].includes(type)) return 'icon-green';
    if ([NotificationType.TimesheetRejected, NotificationType.MissedPunch].includes(type)) return 'icon-red';
    if ([NotificationType.OvertimeAlert, NotificationType.LowPTOBalance].includes(type)) return 'icon-yellow';
    return 'icon-blue';
  }
}
