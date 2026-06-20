import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PTORequest, PTOBalance,
  SubmitPTORequest, ApprovePTORequest, RejectPTORequest
} from '../models/pto.models';

@Injectable({ providedIn: 'root' })
export class PTOService {
  private readonly http = inject(HttpClient);
  private readonly url  = '/pto';

  submit(req: SubmitPTORequest): Observable<PTORequest> {
    return this.http.post<PTORequest>(this.url, req);
  }
  getMy(): Observable<PTORequest[]> {
    return this.http.get<PTORequest[]>(`${this.url}/my`);
  }
  getBalance(): Observable<PTOBalance> {
    return this.http.get<PTOBalance>(`${this.url}/balance`);
  }
  getPending(): Observable<PTORequest[]> {
    return this.http.get<PTORequest[]>(`${this.url}/pending`);
  }
  approve(id: string, req: ApprovePTORequest): Observable<PTORequest> {
    return this.http.post<PTORequest>(`${this.url}/${id}/approve`, req);
  }
  reject(id: string, req: RejectPTORequest): Observable<PTORequest> {
    return this.http.post<PTORequest>(`${this.url}/${id}/reject`, req);
  }
  cancel(id: string): Observable<PTORequest> {
    return this.http.post<PTORequest>(`${this.url}/${id}/cancel`, {});
  }
}
