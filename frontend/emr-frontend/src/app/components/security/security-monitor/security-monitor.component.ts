// security-monitor.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { interval, Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';

interface SecurityMetrics {
  activeUsers: number;
  failedLogins: number;
  suspiciousActivities: number;
  blockedIPs: number;
  avgRiskScore: number;
}

@Component({
  selector: 'app-security-monitor',
  templateUrl: './security-monitor.component.html',
  styleUrls: ['./security-monitor.component.css']
})
export class SecurityMonitorComponent implements OnInit, OnDestroy {
  metrics: SecurityMetrics = {
    activeUsers: 0,
    failedLogins: 0,
    suspiciousActivities: 0,
    blockedIPs: 0,
    avgRiskScore: 0
  };

  private subscription?: Subscription;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadMetrics();
    
    // Auto-refresh every 10 seconds
    this.subscription = interval(10000).subscribe(() => {
      this.loadMetrics();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  loadMetrics(): void {
    this.http.get<SecurityMetrics>('http://localhost:5000/api/security/metrics').subscribe(
      data => {
        this.metrics = data;
      },
      error => console.error('Error loading metrics:', error)
    );
  }

  getRiskLevel(): string {
    if (this.metrics.avgRiskScore >= 70) return 'Cao';
    if (this.metrics.avgRiskScore >= 40) return 'Trung bình';
    return 'Thấp';
  }

  getRiskColor(): string {
    if (this.metrics.avgRiskScore >= 70) return 'warn';
    if (this.metrics.avgRiskScore >= 40) return 'accent';
    return 'primary';
  }
}