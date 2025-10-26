// threat-intelligence.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ThreatIndicator {
  type: string;
  value: string;
  severity: number;
  detectedAt: Date;
}

export interface ThreatAssessment {
  isThreat: boolean;
  threatScore: number;
  threatCategories: string[];
  countryCode: string;
  isVPN: boolean;
  isProxy: boolean;
  isTor: boolean;
}

export interface IpBlacklist {
  id: number;
  ipAddress: string;
  reason: string;
  blockedAt: Date;
  expiresAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ThreatIntelligenceService {
  private apiUrl = 'http://localhost:5000/api/threat-intelligence';

  constructor(private http: HttpClient) {}

  getActiveThreatIndicators(): Observable<ThreatIndicator[]> {
    return this.http.get<ThreatIndicator[]>(`${this.apiUrl}/indicators`);
  }

  assessIpAddress(ipAddress: string): Observable<ThreatAssessment> {
    return this.http.post<ThreatAssessment>(`${this.apiUrl}/assess`, { ipAddress });
  }

  getBlacklist(): Observable<IpBlacklist[]> {
    return this.http.get<IpBlacklist[]>(`${this.apiUrl}/blacklist`);
  }

  blockIpAddress(ipAddress: string, reason: string, duration?: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/block`, {
      ipAddress,
      reason,
      durationHours: duration
    });
  }

  unblockIpAddress(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/blacklist/${id}`);
  }
}