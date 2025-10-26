// audit.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLog {
  id: number;
  userId: number;
  userName: string;
  action: string;
  entityType: string;
  entityId: number;
  oldValues: any;
  newValues: any;
  timestamp: Date;
  ipAddress: string;
  userAgent: string;
  additionalInfo: string;
  isSuccess: boolean;
  failureReason: string;
}

export interface AuditLogFilter {
  userId?: number;
  action?: string;
  entityType?: string;
  startDate?: Date;
  endDate?: Date;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private apiUrl = 'http://localhost:5000/api/audit';

  constructor(private http: HttpClient) {}

  getAuditLogs(filter: AuditLogFilter): Observable<AuditLog[]> {
    let params = new HttpParams();
    
    if (filter.userId) params = params.set('userId', filter.userId.toString());
    if (filter.action) params = params.set('action', filter.action);
    if (filter.entityType) params = params.set('entityType', filter.entityType);
    if (filter.startDate) params = params.set('startDate', filter.startDate.toISOString());
    if (filter.endDate) params = params.set('endDate', filter.endDate.toISOString());
    if (filter.pageNumber) params = params.set('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<AuditLog[]>(this.apiUrl, { params });
  }

  getEntityHistory(entityType: string, entityId: number): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.apiUrl}/entity/${entityType}/${entityId}`);
  }

  exportAuditLogs(filter: AuditLogFilter): Observable<Blob> {
    let params = new HttpParams();
    // Add filter params
    
    return this.http.get(`${this.apiUrl}/export`, { 
      params, 
      responseType: 'blob' 
    });
  }
}