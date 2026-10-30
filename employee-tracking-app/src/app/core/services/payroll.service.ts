import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PayrollReport } from '../models/payroll.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/reports/payroll`;

  get(payPeriodId: string): Observable<PayrollReport> {
    return this.http.get<PayrollReport>(`${this.url}?payPeriodId=${payPeriodId}`);
  }
  generate(payPeriodId: string): Observable<PayrollReport> {
    return this.http.post<PayrollReport>(`${this.url}/generate?payPeriodId=${payPeriodId}`, {});
  }
}
