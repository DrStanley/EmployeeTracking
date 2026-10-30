import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AppActivityReport,
  RecordAppUsageRequest,
  RecordAppUsageResult,
  UserActivitySummary
} from '../models/activity.models';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private readonly http = inject(HttpClient);
  private readonly url = '/activity';

  getReport(
    range: { from: string; to: string },
    employeeId?: string | null
  ): Observable<AppActivityReport> {
    let params = new HttpParams().set('from', range.from).set('to', range.to);
    if (employeeId) params = params.set('employeeId', employeeId);
    return this.http.get<AppActivityReport>(this.url, { params });
  }

  getUsers(range: { from: string; to: string }): Observable<UserActivitySummary[]> {
    const params = new HttpParams().set('from', range.from).set('to', range.to);
    return this.http.get<UserActivitySummary[]>(`${this.url}/users`, { params });
  }

  record(req: RecordAppUsageRequest, background = false): Observable<RecordAppUsageResult> {
    const headers = background
      ? new HttpHeaders({ 'X-Skip-Loading': '1' })
      : undefined;
    return this.http.post<RecordAppUsageResult>(this.url, req, { headers });
  }
}
