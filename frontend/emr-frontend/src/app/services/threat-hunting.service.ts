// threat-hunting.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ThreatHuntingQuery {
  id: number;
  name: string;
  description: string;
  queryDefinition: string;
  severity: string;
  isActive: boolean;
  createdAt: Date;
  lastExecutedAt?: Date;
  executionCount: number;
}

export interface ThreatHuntingResult {
  id: number;
  queryId: number;
  executionTime: Date;
  matchCount: number;
  results: string;
  hasThreats: boolean;
  summary: string;
}

export interface ThreatIndicator {
  id: number;
  indicatorType: string;
  value: string;
  description: string;
  severity: string;
  source: string;
  addedAt: Date;
  matchCount: number;
  isActive: boolean;
}

export interface SuspiciousActivity {
  activityType: string;
  description: string;
  userId: number;
  userName: string;
  timestamp: Date;
  threatScore: number;
  indicators: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ThreatHuntingService {
  private apiUrl = 'http://localhost:5000/api/threat-hunting';

  constructor(private http: HttpClient) {}

  getQueries(): Observable<ThreatHuntingQuery[]> {
    return this.http.get<ThreatHuntingQuery[]>(`${this.apiUrl}/queries`);
  }

  createQuery(query: any): Observable<ThreatHuntingQuery> {
    return this.http.post<ThreatHuntingQuery>(`${this.apiUrl}/queries`, query);
  }

  executeQuery(queryId: number): Observable<ThreatHuntingResult> {
    return this.http.post<ThreatHuntingResult>(`${this.apiUrl}/execute/${queryId}`, {});
  }

  getResults(queryId: number): Observable<ThreatHuntingResult[]> {
    return this.http.get<ThreatHuntingResult[]>(`${this.apiUrl}/results/${queryId}`);
  }

  getIndicators(): Observable<ThreatIndicator[]> {
    return this.http.get<ThreatIndicator[]>(`${this.apiUrl}/indicators`);
  }

  addIndicator(indicator: any): Observable<ThreatIndicator> {
    return this.http.post<ThreatIndicator>(`${this.apiUrl}/indicators`, indicator);
  }

  huntSuspiciousActivities(criteria: any): Observable<SuspiciousActivity[]> {
    return this.http.post<SuspiciousActivity[]>(`${this.apiUrl}/hunt`, criteria);
  }

  getSummary(startDate: Date, endDate: Date): Observable<any> {
    const start = startDate.toISOString().split('T')[0];
    const end = endDate.toISOString().split('T')[0];
    return this.http.get(`${this.apiUrl}/summary?startDate=${start}&endDate=${end}`);
  }
}