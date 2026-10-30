import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AuditLogEntry, CalendarMonth, EmployeeProfile, Schedule
} from '../models/workspace.models';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly http = inject(HttpClient);

  profile(): Observable<EmployeeProfile> {
    return this.http.get<EmployeeProfile>('/employees/me');
  }

  schedule(): Observable<Schedule> {
    return this.http.get<Schedule>('/employees/schedule');
  }

  calendar(year: number, month: number): Observable<CalendarMonth> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<CalendarMonth>('/pto/calendar', { params });
  }

  audit(): Observable<AuditLogEntry[]> {
    return this.http.get<AuditLogEntry[]>('/admin/audit');
  }
}
