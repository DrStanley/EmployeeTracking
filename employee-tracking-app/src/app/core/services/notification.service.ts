import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Notification } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly url  = '/notifications';

  getAll(unreadOnly = false): Observable<Notification[]> {
    const params = unreadOnly ? '?unreadOnly=true' : '';
    return this.http.get<Notification[]>(`${this.url}${params}`);
  }
  markRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/read`, {});
  }
  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.url}/read-all`, {});
  }
}
