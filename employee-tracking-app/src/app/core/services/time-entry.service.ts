import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClockInRequest, ClockInResponse, ClockOutResponse, TimeEntry } from '../models/time-entry.models';
import { Presence, WeeklyHours, WorkedHours } from '../models/workspace.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TimeEntryService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/TimeEntry`;

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
  getWeek(): Observable<WeeklyHours> {
    return this.http.get<WeeklyHours>(`${this.url}/week`);
  }
  getWorked(from: string, to: string, employeeId?: string | null): Observable<WorkedHours> {
    let params = new HttpParams().set('from', from).set('to', to);
    if (employeeId) params = params.set('employeeId', employeeId);
    return this.http.get<WorkedHours>(`${this.url}/worked`, { params });
  }
  getPresence(): Observable<Presence[]> {
    return this.http.get<Presence[]>(`${this.url}/presence`);
  }
}
