import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PayrollReport } from '../models/payroll.models';

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private readonly http = inject(HttpClient);

  get(payPeriodId: string): Observable<PayrollReport> {
    return this.http.get<PayrollReport>(`/reports/payroll?payPeriodId=${payPeriodId}`);
  }
  generate(payPeriodId: string): Observable<PayrollReport> {
    return this.http.post<PayrollReport>(`/reports/payroll/generate?payPeriodId=${payPeriodId}`, {});
  }
}
