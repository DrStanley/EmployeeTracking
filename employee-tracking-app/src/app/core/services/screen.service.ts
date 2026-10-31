import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ScreenCaptureConfig {
  enabled: boolean;
  intervalMinutes: number;
}

export interface ScreenReview {
  id: string;
  employeeId: string;
  employeeName: string;
  startedAt: string;
  endedAt: string;
  frameCount: number;
  findings: string[];
  status: 'Ready' | 'Running' | 'Failed';
}

@Injectable({ providedIn: 'root' })
export class ScreenService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/screen-capture`;

  config(): Observable<ScreenCaptureConfig> {
    return this.http.get<ScreenCaptureConfig>(`${this.url}/config`);
  }

  updateConfig(config: ScreenCaptureConfig): Observable<ScreenCaptureConfig> {
    return this.http.put<ScreenCaptureConfig>(`${this.url}/config`, config);
  }

  uploadFrame(image: Blob, capturedAt: string): Observable<{ id: string; storageKey: string }> {
    const body = new FormData();
    body.append('image', image, 'screen.jpg');
    body.append('capturedAt', capturedAt);
    return this.http.post<{ id: string; storageKey: string }>(`${this.url}/frames`, body, {
      headers: { 'X-Skip-Loading': '1' }
    });
  }

  completeReview(startedAt: string, endedAt: string): Observable<{ id: string; frameCount: number }> {
    return this.http.post<{ id: string; frameCount: number }>(`${this.url}/reviews`, { startedAt, endedAt }, {
      headers: { 'X-Skip-Loading': '1' }
    });
  }

  reviews(): Observable<ScreenReview[]> {
    return this.http.get<ScreenReview[]>(`${this.url}/reviews`);
  }
}
