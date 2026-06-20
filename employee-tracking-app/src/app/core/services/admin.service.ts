import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AttendancePolicy, OvertimeRule, Shift, Holiday,
  PayPeriod, PTOAccrualSettings
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = '/admin';

  // Policies
  getPolicies(): Observable<AttendancePolicy[]>     { return this.http.get<AttendancePolicy[]>(`${this.base}/policies`); }
  getPolicy(id: string): Observable<AttendancePolicy> { return this.http.get<AttendancePolicy>(`${this.base}/policies/${id}`); }
  createPolicy(b: Partial<AttendancePolicy>): Observable<AttendancePolicy>      { return this.http.post<AttendancePolicy>(`${this.base}/policies`, b); }
  updatePolicy(id: string, b: Partial<AttendancePolicy>): Observable<AttendancePolicy> { return this.http.put<AttendancePolicy>(`${this.base}/policies/${id}`, b); }
  deletePolicy(id: string): Observable<void>        { return this.http.delete<void>(`${this.base}/policies/${id}`); }
  getPTOSettings(policyId: string): Observable<PTOAccrualSettings> { return this.http.get<PTOAccrualSettings>(`${this.base}/policies/${policyId}/pto-settings`); }
  updatePTOSettings(policyId: string, b: Partial<PTOAccrualSettings>): Observable<PTOAccrualSettings> { return this.http.put<PTOAccrualSettings>(`${this.base}/policies/${policyId}/pto-settings`, b); }

  // Overtime Rules
  getOvertimeRules(): Observable<OvertimeRule[]>    { return this.http.get<OvertimeRule[]>(`${this.base}/overtime-rules`); }
  createOvertimeRule(b: Partial<OvertimeRule>): Observable<OvertimeRule>        { return this.http.post<OvertimeRule>(`${this.base}/overtime-rules`, b); }
  updateOvertimeRule(id: string, b: Partial<OvertimeRule>): Observable<OvertimeRule> { return this.http.put<OvertimeRule>(`${this.base}/overtime-rules/${id}`, b); }
  deleteOvertimeRule(id: string): Observable<void>  { return this.http.delete<void>(`${this.base}/overtime-rules/${id}`); }

  // Shifts
  getShifts(): Observable<Shift[]>                 { return this.http.get<Shift[]>(`${this.base}/shifts`); }
  createShift(b: Partial<Shift>): Observable<Shift>{ return this.http.post<Shift>(`${this.base}/shifts`, b); }
  updateShift(id: string, b: Partial<Shift>): Observable<Shift> { return this.http.put<Shift>(`${this.base}/shifts/${id}`, b); }
  deleteShift(id: string): Observable<void>        { return this.http.delete<void>(`${this.base}/shifts/${id}`); }

  // Holidays
  getHolidays(year?: number): Observable<Holiday[]>{ return this.http.get<Holiday[]>(`${this.base}/holidays${year ? '?year=' + year : ''}`); }
  createHoliday(b: Partial<Holiday>): Observable<Holiday> { return this.http.post<Holiday>(`${this.base}/holidays`, b); }
  updateHoliday(id: string, b: Partial<Holiday>): Observable<Holiday> { return this.http.put<Holiday>(`${this.base}/holidays/${id}`, b); }
  deleteHoliday(id: string): Observable<void>      { return this.http.delete<void>(`${this.base}/holidays/${id}`); }

  // Pay Periods
  getPayPeriods(): Observable<PayPeriod[]>          { return this.http.get<PayPeriod[]>(`${this.base}/pay-periods`); }
  createPayPeriod(b: Partial<PayPeriod>): Observable<PayPeriod> { return this.http.post<PayPeriod>(`${this.base}/pay-periods`, b); }
  lockPayPeriod(id: string): Observable<PayPeriod> { return this.http.post<PayPeriod>(`${this.base}/pay-periods/${id}/lock`, {}); }
}
