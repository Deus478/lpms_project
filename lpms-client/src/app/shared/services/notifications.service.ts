import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Notification {
  notificationId: number;
  userId: number;
  title: string;
  message: string;
  type: string;
  priority: string;
  entityType?: string;
  entityId?: number;
  actionUrl?: string;
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
  expiresAt?: Date;
}

export interface NotificationPreference {
  notificationPreferenceId: number;
  userId: number;
  notificationType: string;
  emailEnabled: boolean;
  inAppEnabled: boolean;
  smsEnabled: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/notifications`;

  constructor(private http: HttpClient) {}

  // Notifications
  getUserNotifications(userId: number, unreadOnly: boolean = false): Observable<Notification[]> {
    let params = new HttpParams();
    if (unreadOnly) {
      params = params.set('unreadOnly', 'true');
    }
    return this.http.get<Notification[]>(`${this.apiUrl}/users/${userId}`, { params });
  }

  markAsRead(notificationId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${notificationId}/read`, {});
  }

  markAllAsRead(userId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/${userId}/read-all`, {});
  }

  createNotification(notification: Partial<Notification>): Observable<Notification> {
    return this.http.post<Notification>(this.apiUrl, notification);
  }

  getUnreadCount(userId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/users/${userId}/unread-count`);
  }

  // Preferences
  getNotificationPreferences(userId: number): Observable<NotificationPreference[]> {
    return this.http.get<NotificationPreference[]>(`${this.apiUrl}/preferences/${userId}`);
  }

  updateNotificationPreferences(userId: number, preferences: any[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/preferences/${userId}`, preferences);
  }

  // Cleanup
  cleanupOldNotifications(daysOld: number = 30): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/cleanup?daysOld=${daysOld}`);
  }
}
