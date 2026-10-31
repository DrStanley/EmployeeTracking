import { Component, inject, signal, OnInit, OnDestroy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { interval, Subscription } from 'rxjs';
import { TimeEntryService } from '../../core/services/time-entry.service';
import { apiErrorMessage } from '../../core/http/api-error';
import { ClockStatus, TimeEntrySource } from '../../core/models/time-entry.models';
import { AuthStore } from '../../core/auth/auth.store';
import { ScreenService } from '../../core/services/screen.service';
import { ScreenSessionService } from '../../core/screen/screen-session.service';

const STORAGE_KEYS = {
  status: 'et_clockStatus',
  clockedInAt: 'et_clockedInAt',
  breakStartedAt: 'et_breakStartedAt'
} as const;

@Component({
  selector: 'app-time-tracking',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './time-tracking.component.html',
  styleUrl: './time-tracking.component.scss'
})
export class TimeTrackingComponent implements OnInit, OnDestroy {
  private readonly timeEntryService = inject(TimeEntryService);
  private readonly screenApi = inject(ScreenService);
  private readonly screenSession = inject(ScreenSessionService);
  private readonly auth = inject(AuthStore);
  private tickSub?: Subscription;

  now = signal(new Date());
  status = signal<ClockStatus>('clocked-out');
  clockedInAt = signal<Date | null>(null);
  breakStartedAt = signal<Date | null>(null);
  includeLocation = signal(false);
  source = signal<TimeEntrySource>(TimeEntrySource.WebApp);
  loading = signal(false);
  errorMsg = signal<string | null>(null);
  notice = signal<string | null>(null);
  captureEnabled = signal(false);
  captureInterval = signal(5);
  todayEntries = signal<any[]>([]);

  sources = [
    { value: TimeEntrySource.WebApp, label: 'Web App' },
    { value: TimeEntrySource.MobileApp, label: 'Mobile App' },
    { value: TimeEntrySource.Kiosk, label: 'Kiosk' }
  ];

  duration = computed(() => {
    const st = this.status();
    const start = st === 'on-break' ? this.breakStartedAt() : this.clockedInAt();
    if (!start) return '0h 0m';
    const diffMs = this.now().getTime() - start.getTime();
    const hours = Math.floor(diffMs / 3600000);
    const mins = Math.floor((diffMs % 3600000) / 60000);
    return `${hours}h ${mins}m`;
  });

  // ── Lifecycle ──────────────────────────────────────────────────────────────

  readonly session = this.screenSession;

  ngOnInit(): void {
    this.tickSub = interval(1000).subscribe(() => this.now.set(new Date()));
    this.screenApi.config().subscribe({
      next: config => {
        this.captureEnabled.set(config.enabled);
        this.captureInterval.set(config.intervalMinutes);
      },
      error: () => this.captureEnabled.set(false)
    });
    this.loadTodayEntries();

    // Always load real status from API — works across devices and tabs
    this.timeEntryService.getClockStatus().subscribe({
      next: res => {
        if (res.status === 'clocked-in' && res.clockedInAt) {
          this.status.set('clocked-in');
          this.clockedInAt.set(new Date(res.clockedInAt));
          this.saveStatus('clocked-in', new Date(res.clockedInAt), null);
        } else if (res.status === 'on-break' && res.clockedInAt) {
          this.status.set('on-break');
          this.clockedInAt.set(new Date(res.clockedInAt));
          const breakAt = res.breakStartedAt ? new Date(res.breakStartedAt) : new Date();
          this.breakStartedAt.set(breakAt);
          this.saveStatus('on-break', new Date(res.clockedInAt), breakAt);
        } else {
          this.status.set('clocked-out');
          this.clearStorage();
        }
      },
      error: () => {
        // Fallback to sessionStorage if API is temporarily unreachable
        this.restoreFromStorage();
      }
    });
  }
  loadTodayEntries(): void {
    this.timeEntryService.getTodayEntries().subscribe({
      next: entries => this.todayEntries.set(entries),
      error: () => { }
    });
  }
  ngOnDestroy(): void {
    this.tickSub?.unsubscribe();
  }

  // ── Storage ────────────────────────────────────────────────────────────────

  private restoreFromStorage(): void {
    const saved = sessionStorage.getItem(STORAGE_KEYS.status) as ClockStatus | null;
    const inAt = sessionStorage.getItem(STORAGE_KEYS.clockedInAt);
    const breakAt = sessionStorage.getItem(STORAGE_KEYS.breakStartedAt);

    if (saved === 'clocked-in' && inAt) {
      this.status.set('clocked-in');
      this.clockedInAt.set(new Date(inAt));
    } else if (saved === 'on-break' && inAt && breakAt) {
      this.status.set('on-break');
      this.clockedInAt.set(new Date(inAt));
      this.breakStartedAt.set(new Date(breakAt));
    }
  }

  private saveStatus(
    status: ClockStatus,
    clockedInAt?: Date | null,
    breakStartedAt?: Date | null
  ): void {
    sessionStorage.setItem(STORAGE_KEYS.status, status);
    clockedInAt
      ? sessionStorage.setItem(STORAGE_KEYS.clockedInAt, clockedInAt.toISOString())
      : sessionStorage.removeItem(STORAGE_KEYS.clockedInAt);
    breakStartedAt
      ? sessionStorage.setItem(STORAGE_KEYS.breakStartedAt, breakStartedAt.toISOString())
      : sessionStorage.removeItem(STORAGE_KEYS.breakStartedAt);
  }

  private clearStorage(): void {
    Object.values(STORAGE_KEYS).forEach(k => sessionStorage.removeItem(k));
  }

  // ── Location ───────────────────────────────────────────────────────────────

  private getLocation(): Promise<{ lat?: number; lng?: number }> {
    if (!this.includeLocation() || !navigator.geolocation)
      return Promise.resolve({});
    return new Promise(resolve => {
      navigator.geolocation.getCurrentPosition(
        pos => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
        () => resolve({})
      );
    });
  }

  toggleLocation(): void { this.includeLocation.update(v => !v); }

  // ── Actions ────────────────────────────────────────────────────────────────

  async clockIn(): Promise<void> {
    this.loading.set(true);
    this.errorMsg.set(null);
    this.notice.set(null);
    if (this.captureEnabled()) {
      try {
        await this.screenSession.begin(this.captureInterval());
      } catch (err) {
        this.errorMsg.set(err instanceof Error ? err.message : 'Share your entire screen to clock in.');
        this.loading.set(false);
        return;
      }
    }
    const loc = await this.getLocation();

    this.timeEntryService.clockIn({ source: this.source(), latitude: loc.lat, longitude: loc.lng })
      .subscribe({
        next: res => {
          const ts = new Date(res.timestamp);
          this.status.set('clocked-in');
          this.clockedInAt.set(ts);
          this.breakStartedAt.set(null);
          this.saveStatus('clocked-in', ts, null);
          this.loadTodayEntries();
          this.loading.set(false);
        },
        error: err => {
          this.screenSession.cancel();
          this.errorMsg.set(apiErrorMessage(err, 'Failed to clock in.'));
          this.loading.set(false);
        }
      });


  }

  async clockOut(): Promise<void> {
    this.loading.set(true);
    this.errorMsg.set(null);
    this.notice.set(null);
    const reviewing = this.screenSession.isPending();
    const started = this.clockedInAt()?.toISOString() ?? new Date().toISOString();
    if (reviewing) this.screenSession.stopSharing();
    const loc = await this.getLocation();

    this.timeEntryService.clockOut({ source: this.source(), latitude: loc.lat, longitude: loc.lng })
      .subscribe({
        next: () => {
          this.status.set('clocked-out');
          this.clockedInAt.set(null);
          this.breakStartedAt.set(null);
          this.clearStorage();
          this.loadTodayEntries();
          this.loading.set(false);
          if (!reviewing) return;
          this.notice.set('Clocked out. The screen review is running and will be ready for a manager or admin.');
          this.screenApi.completeReview(started, new Date().toISOString()).subscribe({
            next: () => this.screenSession.clear(),
            error: err => this.errorMsg.set(apiErrorMessage(err, 'Clock-out was saved. The screen review could not be scheduled.'))
          });
        },
        error: err => {
          this.errorMsg.set(apiErrorMessage(err, 'Failed to clock out.'));
          this.loading.set(false);
        }
      });

  }

  startBreak(): void {
    this.loading.set(true);
    this.timeEntryService.breakStart().subscribe({
      next: () => {
        const now = new Date();
        this.status.set('on-break');
        this.breakStartedAt.set(now);
        this.saveStatus('on-break', this.clockedInAt(), now);
        this.loadTodayEntries();
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Failed to start break.'));
        this.loading.set(false);
      }
    });

  }

  endBreak(): void {
    this.loading.set(true);
    this.timeEntryService.breakEnd().subscribe({
      next: () => {
        this.status.set('clocked-in');
        this.breakStartedAt.set(null);
        this.saveStatus('clocked-in', this.clockedInAt(), null);
        this.loading.set(false);
        this.loadTodayEntries();
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Failed to end break.'));
        this.loading.set(false);
      }
    });

  }
}