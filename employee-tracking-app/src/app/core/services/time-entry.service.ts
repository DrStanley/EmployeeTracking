import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClockInRequest, ClockInResponse, ClockOutResponse, TimeEntry } from '../models/time-entry.models';

@Injectable({ providedIn: 'root' })
export class TimeEntryService {
  private readonly http = inject(HttpClient);
  private readonly url = '/TimeEntry';

  clockIn(req: ClockInRequest): Observable<ClockInResponse> {
    return this.http.post<ClockInResponse>(`${this.url}/clock-in`, req);
  }
  clockOut(req: ClockInRequest): Observable<ClockOutResponse> {
    return this.http.post<ClockOutResponse>(`${this.url}/clock-out`, req);
  }
  breakStart(): Observable<ClockInResponse> {
    return this.http.post<ClockInResponse>(`${this.url}/break-start`, {});
  }
  breakEnd(): Observable<ClockInResponse> {
    return this.http.post<ClockInResponse>(`${this.url}/break-end`, {});
  }
  getMyEntries(): Observable<TimeEntry[]> {
    return this.http.get<TimeEntry[]>(`${this.url}/my`);
  }
  getClockStatus(): Observable<{ status: string; clockedInAt?: string; breakStartedAt?: string }> {
    return this.http.get<{ status: string; clockedInAt?: string; breakStartedAt?: string }>(
      `${this.url}/clock-status`
    );
  }
  getTodayEntries(): Observable<any[]> {
    return this.http.get<any[]>(`${this.url}/today`);
  }
}
