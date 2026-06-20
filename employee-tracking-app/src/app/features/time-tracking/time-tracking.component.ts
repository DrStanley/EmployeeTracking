import { Component, inject, signal, OnInit, OnDestroy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { interval, Subscription } from 'rxjs';
import { TimeEntryService } from '../../core/services/time-entry.service';
import { ClockStatus, TimeEntrySource } from '../../core/models/time-entry.models';

@Component({
  selector: 'app-time-tracking',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './time-tracking.component.html',
  styleUrl: './time-tracking.component.scss'
})
export class TimeTrackingComponent implements OnInit, OnDestroy {
  private readonly timeEntryService = inject(TimeEntryService);
  private tickSub?: Subscription;

  now = signal(new Date());
  status = signal<ClockStatus>('clocked-out');
  clockedInAt = signal<Date | null>(null);
  breakStartedAt = signal<Date | null>(null);
  includeLocation = signal(false);
  source = signal<TimeEntrySource>(TimeEntrySource.WebApp);
  loading = signal(false);
  errorMsg = signal<string | null>(null);

  sources = [
    { value: TimeEntrySource.WebApp, label: 'Web App' },
    { value: TimeEntrySource.MobileApp, label: 'Mobile App' },
    { value: TimeEntrySource.Kiosk, label: 'Kiosk' }
  ];

  duration = computed(() => {
    const start = this.status() === 'on-break' ? this.breakStartedAt() : this.clockedInAt();
    if (!start) return '0h 0m';
    const diffMs = this.now().getTime() - start.getTime();
    const hours = Math.floor(diffMs / 3600000);
    const mins = Math.floor((diffMs % 3600000) / 60000);
    return `${hours}h ${mins}m`;
  });

  ngOnInit(): void {
    this.tickSub = interval(1000).subscribe(() => this.now.set(new Date()));
  }

  ngOnDestroy(): void {
    this.tickSub?.unsubscribe();
  }

  private getLocation(): Promise<{ lat?: number; lng?: number }> {
    if (!this.includeLocation() || !navigator.geolocation) return Promise.resolve({});
    return new Promise(resolve => {
      navigator.geolocation.getCurrentPosition(
        pos => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
        () => resolve({})
      );
    });
  }

  async clockIn(): Promise<void> {
    this.loading.set(true);
    this.errorMsg.set(null);
    const loc = await this.getLocation();
    this.timeEntryService.clockIn({
      source: this.source(),
      latitude: loc.lat,
      longitude: loc.lng
    }).subscribe({
      next: res => {
        this.status.set('clocked-in');
        this.clockedInAt.set(new Date(res.timestamp));
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to clock in.');
        this.loading.set(false);
      }
    });
  }

  async clockOut(): Promise<void> {
    this.loading.set(true);
    this.errorMsg.set(null);
    const loc = await this.getLocation();
    this.timeEntryService.clockOut({
      source: this.source(),
      latitude: loc.lat,
      longitude: loc.lng
    }).subscribe({
      next: () => {
        this.status.set('clocked-out');
        this.clockedInAt.set(null);
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to clock out.');
        this.loading.set(false);
      }
    });
  }

  startBreak(): void {
    this.loading.set(true);
    this.timeEntryService.breakStart().subscribe({
      next: () => {
        this.status.set('on-break');
        this.breakStartedAt.set(new Date());
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to start break.');
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
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(err.error?.detail ?? 'Failed to end break.');
        this.loading.set(false);
      }
    });
  }

  toggleLocation(): void {
    this.includeLocation.update(v => !v);
  }
}
