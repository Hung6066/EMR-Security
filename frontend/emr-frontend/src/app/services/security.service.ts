// security.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SecurityService {
  private apiUrl = 'http://localhost:5000/api/security';

  constructor(private http: HttpClient) {}

  getActiveSessions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sessions`);
  }

  revokeSession(sessionId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/sessions/${sessionId}/revoke`, {});
  }

  revokeAllSessions(exceptSessionId?: number): Observable<void> {
    const params = exceptSessionId ? { exceptSessionId } : {};
    return this.http.post<void>(`${this.apiUrl}/sessions/revoke-all`, {}, { params });
  }

  getLoginHistory(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/login-history`);
  }
}