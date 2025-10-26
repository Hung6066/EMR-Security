// zero-trust.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TrustScore {
  overallScore: number;
  deviceScore: number;
  locationScore: number;
  behaviorScore: number;
  timeScore: number;
  networkScore: number;
  details: { [key: string]: string };
}

export interface AccessDecision {
  isAllowed: boolean;
  trustScore: number;
  denialReasons: string[];
  appliedPolicies: string[];
  requiredActions: { [key: string]: any };
}

export interface ZeroTrustPolicy {
  id: number;
  name: string;
  description: string;
  resourceType: string;
  resourcePath: string;
  minTrustScore: number;
  requiresMFA: boolean;
  requiresDeviceCompliance: boolean;
  requiresNetworkCompliance: boolean;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ZeroTrustService {
  private apiUrl = 'http://localhost:5000/api/zero-trust';

  constructor(private http: HttpClient) {}

  getTrustScore(): Observable<TrustScore> {
    return this.http.get<TrustScore>(`${this.apiUrl}/trust-score`);
  }

  getPolicies(): Observable<ZeroTrustPolicy[]> {
    return this.http.get<ZeroTrustPolicy[]>(`${this.apiUrl}/policies`);
  }

  createPolicy(policy: Partial<ZeroTrustPolicy>): Observable<ZeroTrustPolicy> {
    return this.http.post<ZeroTrustPolicy>(`${this.apiUrl}/policies`, policy);
  }

  updatePolicy(id: number, policy: Partial<ZeroTrustPolicy>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/policies/${id}`, policy);
  }

  deletePolicy(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/policies/${id}`);
  }

  getAccessHistory(): Observable<AccessDecision[]> {
    return this.http.get<AccessDecision[]>(`${this.apiUrl}/access-history`);
  }
}