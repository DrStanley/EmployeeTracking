import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AuditLogEntry, CalendarMonth, EmployeeProfile, PersonalDetails, Schedule
} from '../models/workspace.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly http = inject(HttpClient);

  profile(): Observable<EmployeeProfile> {
    return this.http.get<EmployeeProfile>(`${environment.apiUrl}/employees/me`);
  }

  updateProfile(details: PersonalDetails): Observable<EmployeeProfile> {
    return this.http.put<EmployeeProfile>(`${environment.apiUrl}/employees/me`, details);
  }

  schedule(): Observable<Schedule> {
    return this.http.get<Schedule>(`${environment.apiUrl}/employees/schedule`);
  }

  calendar(year: number, month: number): Observable<CalendarMonth> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<CalendarMonth>(`${environment.apiUrl}/pto/calendar`, { params });
  }

  audit(): Observable<AuditLogEntry[]> {
    return this.http.get<AuditLogEntry[]>(`${environment.apiUrl}/admin/audit`);
  }
}
