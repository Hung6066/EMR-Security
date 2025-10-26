// two-factor.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Enable2FAResponse {
  qrCodeUrl: string;
  secretKey: string;
  backupCodes: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TwoFactorService {
  private apiUrl = 'http://localhost:5000/api/twofactor';

  constructor(private http: HttpClient) {}

  enable2FA(): Observable<Enable2FAResponse> {
    return this.http.post<Enable2FAResponse>(`${this.apiUrl}/enable`, {});
  }

  verify2FA(code: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify`, { code });
  }

  disable2FA(password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/disable`, JSON.stringify(password), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  regenerateBackupCodes(): Observable<string[]> {
    return this.http.post<string[]>(`${this.apiUrl}/regenerate-backup-codes`, {});
  }
}