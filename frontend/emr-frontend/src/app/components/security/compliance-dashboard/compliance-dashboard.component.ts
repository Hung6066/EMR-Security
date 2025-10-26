// compliance-dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { ComplianceService, ComplianceReport } from '../../services/compliance.service';

@Component({
  selector: 'app-compliance-dashboard',
  templateUrl: './compliance-dashboard.component.html',
  styleUrls: ['./compliance-dashboard.component.css']
})
export class ComplianceDashboardComponent implements OnInit {
  reports: ComplianceReport[] = [];
  selectedReport: ComplianceReport | null = null;
  loading = true;

  constructor(private complianceService: ComplianceService) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.complianceService.getComplianceReports().subscribe(
      data => {
        this.reports = data;
        if (data.length > 0) {
          this.selectedReport = data[0];
        }
        this.loading = false;
      },
      error => {
        console.error('Error loading compliance reports:', error);
        this.loading = false;
      }
    );
  }

  selectReport(report: ComplianceReport): void {
    this.selectedReport = report;
  }

  runAssessment(standard: string): void {
    this.loading = true;
    this.complianceService.runAssessment(standard).subscribe(
      data => {
        this.loadReports();
      },
      error => {
        console.error('Error running assessment:', error);
        this.loading = false;
      }
    );
  }

  exportReport(standard: string): void {
    this.complianceService.exportReport(standard).subscribe(
      blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `compliance-${standard}-${new Date().getTime()}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error => console.error('Error exporting report:', error)
    );
  }

  getComplianceColor(score: number): string {
    if (score >= 90) return '#4caf50';
    if (score >= 70) return '#2196f3';
    if (score >= 50) return '#ff9800';
    return '#f44336';
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'Compliant': 'primary',
      'PartiallyCompliant': 'accent',
      'NonCompliant': 'warn'
    };
    return colors[status] || '';
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'Compliant': 'check_circle',
      'PartiallyCompliant': 'warning',
      'NonCompliant': 'cancel'
    };
    return icons[status] || 'help';
  }
}