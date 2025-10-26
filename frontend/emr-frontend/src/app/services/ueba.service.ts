import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UebaAlert {
  id: number;
  userId: number;
  userName: string; // Giả sử backend trả về
  alertType: string;
  description: string;
  deviationScore: number;
  detectedAt: Date;
  context: string; // JSON string
  status: string; // New, Investigating, Resolved, FalsePositive
}

export interface UebaMetrics {
  totalAlerts: number;
  unresolvedAlerts: number;
  avgDeviationScore: number;
  alertsByType: { [key: string]: number };
}

@Injectable({
  providedIn: 'root'
})
export class UebaService {
  private apiUrl = 'http://localhost:5000/api/ueba'; // URL của backend

  constructor(private http: HttpClient) {}

  getAlerts(from: string, to: string): Observable<UebaAlert[]> {
    return this.http.get<UebaAlert[]>(`${this.apiUrl}/alerts?from=${from}&to=${to}`);
  }

  getAlertById(id: number): Observable<UebaAlert> {
    return this.http.get<UebaAlert>(`${this.apiUrl}/alerts/${id}`);
  }

  updateAlertStatus(id: number, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/alerts/${id}/status`, { status });
  }

  getMetrics(from: string, to: string): Observable<UebaMetrics> {
    return this.http.get<UebaMetrics>(`${this.apiUrl}/metrics?from=${from}&to=${to}`);
  }
}