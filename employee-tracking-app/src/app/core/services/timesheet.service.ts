import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Timesheet, CreateTimesheetRequest,
  ApproveTimesheetRequest, RejectTimesheetRequest
} from '../models/timesheet.models';

@Injectable({ providedIn: 'root' })
export class TimesheetService {
  private readonly http = inject(HttpClient);
  private readonly url  = '/timesheets';

  create(req: CreateTimesheetRequest): Observable<Timesheet> {
    return this.http.post<Timesheet>(this.url, req);
  }
  get(employeeId: string, payPeriodId: string): Observable<Timesheet> {
    return this.http.get<Timesheet>(`${this.url}/${employeeId}/${payPeriodId}`);
  }
  submit(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/submit`, {});
  }
  approve(id: string, req: ApproveTimesheetRequest): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/approve`, req);
  }
  reject(id: string, req: RejectTimesheetRequest): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/reject`, req);
  }
  getPending(): Observable<Timesheet[]> {
    return this.http.get<Timesheet[]>(`${this.url}/pending`);
  }
}
