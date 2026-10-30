import { inject, Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../auth/auth.store';
import { ActivityService } from './activity.service';

const APP_ROUTES: Array<[string, string]> = [
  ['/admin/overtime-rules', 'Overtime Rules'],
  ['/admin/pay-periods', 'Pay Periods'],
  ['/admin/policies', 'Policies'],
  ['/admin/holidays', 'Holidays'],
  ['/admin/shifts', 'Shifts'],
  ['/time-tracking', 'Time Tracking'],
  ['/timesheets', 'Timesheets'],
  ['/notifications', 'Notifications'],
  ['/approvals', 'Approvals'],
  ['/dashboard', 'Dashboard'],
  ['/activity', 'Activity'],
  ['/payroll', 'Payroll'],
  ['/pto', 'PTO']
];

/**
 * Records time spent in each area of EmpTrack while the employee is signed in
 * and the tab is visible.
 */
@Injectable({ providedIn: 'root' })
export class ActivityTrackerService {
  private readonly router = inject(Router);
  private readonly activity = inject(ActivityService);
  private readonly store = inject(AuthStore);

  private started = false;
  private paused = false;
  private currentApp: string | null = null;
  private cursor: Date | null = null;
  private timer: ReturnType<typeof setInterval> | null = null;
  private routeSub: Subscription | null = null;

  start(): void {
    if (this.started) return;
    this.started = true;
    this.activate(this.router.url);

    this.routeSub = this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe(event => this.activate(event.urlAfterRedirects));

    this.timer = setInterval(() => this.flush(), 60_000);
    document.addEventListener('visibilitychange', this.onVisibility);
    window.addEventListener('pagehide', this.onPageHide);
  }

  stop(): void {
    if (!this.started) return;
    this.flush();
    this.started = false;
    this.currentApp = null;
    this.cursor = null;
    if (this.timer) clearInterval(this.timer);
    this.timer = null;
    this.routeSub?.unsubscribe();
    this.routeSub = null;
    document.removeEventListener('visibilitychange', this.onVisibility);
    window.removeEventListener('pagehide', this.onPageHide);
  }

  private activate(url: string): void {
    const app = this.appNameFor(url);
    if (!app || app === this.currentApp) return;
    this.flush();
    this.currentApp = app;
    this.cursor = this.paused ? null : new Date();
  }

  private flush(): void {
    if (!this.currentApp || !this.cursor || this.paused) return;

    const startedAt = this.cursor;
    const endedAt = new Date();
    const seconds = (endedAt.getTime() - startedAt.getTime()) / 1000;
    if (seconds < 5) return;

    const appName = this.currentApp;
    this.cursor = endedAt;
    this.activity.record({
      appName,
      startedAt: startedAt.toISOString(),
      endedAt: endedAt.toISOString(),
      source: 'System'
    }, true).subscribe({ error: () => { /* keep the page usable if a flush fails */ } });
  }

  private readonly onVisibility = (): void => {
    if (document.visibilityState === 'hidden') {
      this.flush();
      this.paused = true;
      return;
    }
    this.paused = false;
    this.cursor = new Date();
  };

  private readonly onPageHide = (): void => {
    if (!this.currentApp || !this.cursor || this.paused) return;

    const startedAt = this.cursor;
    const endedAt = new Date();
    this.cursor = endedAt;
    this.paused = true;
    if ((endedAt.getTime() - startedAt.getTime()) / 1000 < 5) return;

    const token = this.store.accessToken();
    if (!token) return;

    const body = JSON.stringify({
      appName: this.currentApp,
      startedAt: startedAt.toISOString(),
      endedAt: endedAt.toISOString(),
      source: 'System'
    });

    fetch(`${environment.apiUrl}/activity`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      },
      body,
      keepalive: true
    }).catch(() => { /* tab is closing */ });
  };

  private appNameFor(url: string): string | null {
    const path = url.split('?')[0];
    const match = APP_ROUTES.find(([route]) => path.startsWith(route));
    return match?.[1] ?? null;
  }
}
