// security-incident.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SecurityIncident {
  id: number;
  title: string;
  description: string;
  severity: string;
  status: string;
  category: string;
  affectedUserId?: number;
  affectedResource?: string;
  ipAddress?: string;
  detectedAt: Date;
  acknowledgedAt?: Date;
  resolvedAt?: Date;
  closedAt?: Date;
  assignedToUserId?: number;
  assignedToUserName?: string;
  comments?: IncidentComment[];
  actions?: IncidentAction[];
}

export interface IncidentComment {
  id: number;
  userId: number;
  userName: string;
  comment: string;
  createdAt: Date;
}

export interface IncidentAction {
  id: number;
  actionType: string;
  description: string;
  performedByUserName: string;
  performedAt: Date;
  result: string;
}

export interface CreateIncident {
  title: string;
  description: string;
  severity: string;
  category: string;
  affectedUserId?: number;
  affectedResource?: string;
  ipAddress?: string;
}

export interface IncidentMetrics {
  totalIncidents: number;
  criticalIncidents: number;
  resolvedIncidents: number;
  averageResolutionTime: number;
  incidentsByCategory: { [key: string]: number };
  incidentsBySeverity: { [key: string]: number };
}

@Injectable({
  providedIn: 'root'
})
export class SecurityIncidentService {
  private apiUrl = 'http://localhost:5000/api/security-incidents';

  constructor(private http: HttpClient) {}

  getIncidents(): Observable<SecurityIncident[]> {
    return this.http.get<SecurityIncident[]>(this.apiUrl);
  }

  getActiveIncidents(): Observable<SecurityIncident[]> {
    return this.http.get<SecurityIncident[]>(`${this.apiUrl}/active`);
  }

  getIncidentById(id: number): Observable<SecurityIncident> {
    return this.http.get<SecurityIncident>(`${this.apiUrl}/${id}`);
  }

  createIncident(incident: CreateIncident): Observable<SecurityIncident> {
    return this.http.post<SecurityIncident>(this.apiUrl, incident);
  }

  updateStatus(id: number, status: string, notes?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, { status, notes });
  }

  assignIncident(id: number, userId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/assign`, { userId });
  }

  addComment(id: number, comment: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/comments`, { comment });
  }

  getMetrics(startDate: Date, endDate: Date): Observable<IncidentMetrics> {
    const start = startDate.toISOString().split('T')[0];
    const end = endDate.toISOString().split('T')[0];
    return this.http.get<IncidentMetrics>(`${this.apiUrl}/metrics?startDate=${start}&endDate=${end}`);
  }
}