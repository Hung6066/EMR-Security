// compliance.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ComplianceReport {
  standard: string; // HIPAA, GDPR, ISO27001
  overallScore: number;
  categories: ComplianceCategory[];
  lastAssessment: Date;
  nextAssessment: Date;
}

export interface ComplianceCategory {
  name: string;
  score: number;
  requirements: ComplianceRequirement[];
}

export interface ComplianceRequirement {
  id: string;
  description: string;
  status: string; // Compliant, NonCompliant, PartiallyCompliant
  evidence: string;
  lastChecked: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ComplianceService {
  private apiUrl = 'http://localhost:5000/api/compliance';

  constructor(private http: HttpClient) {}

  getComplianceReports(): Observable<ComplianceReport[]> {
    return this.http.get<ComplianceReport[]>(`${this.apiUrl}/reports`);
  }

  getReportByStandard(standard: string): Observable<ComplianceReport> {
    return this.http.get<ComplianceReport>(`${this.apiUrl}/reports/${standard}`);
  }

  runAssessment(standard: string): Observable<ComplianceReport> {
    return this.http.post<ComplianceReport>(`${this.apiUrl}/assess/${standard}`, {});
  }

  exportReport(standard: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/${standard}`, {
      responseType: 'blob'
    });
  }
}