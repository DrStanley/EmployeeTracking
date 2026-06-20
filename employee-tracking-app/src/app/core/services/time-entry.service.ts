import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClockInRequest, ClockInResponse, ClockOutResponse, TimeEntry } from '../models/time-entry.models';

@Injectable({ providedIn: 'root' })
export class TimeEntryService {
  private readonly http = inject(HttpClient);
  private readonly url  = '/timeentries';

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
}
