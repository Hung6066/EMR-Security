// anomaly-detection.component.ts
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';

interface AnomalyAlert {
  id: number;
  type: string;
  score: number;
  description: string;
  detectedAt: Date;
  isResolved: boolean;
  userId?: number;
  userName?: string;
}

interface AnomalyStats {
  totalAnomalies: number;
  unresolvedAnomalies: number;
  averageScore: number;
  anomaliesByType: { [key: string]: number };
}

@Component({
  selector: 'app-anomaly-detection',
  templateUrl: './anomaly-detection.component.html',
  styleUrls: ['./anomaly-detection.component.css']
})
export class AnomalyDetectionComponent implements OnInit {
  anomalies: AnomalyAlert[] = [];
  stats: AnomalyStats | null = null;
  
  displayedColumns = ['score', 'type', 'description', 'detectedAt', 'user', 'status', 'actions'];
  
  chartData: any[] = [];
  chartOptions: any;

  constructor(private http: HttpClient) {
    this.setupChart();
  }

  ngOnInit(): void {
    this.loadAnomalies();
    this.loadStats();

    // Auto-refresh every 30 seconds
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.http.get<AnomalyAlert[]>('http://localhost:5000/api/anomalies/recent'))
    ).subscribe(
      data => {
        this.anomalies = data;
        this.updateChart(data);
      }
    );
  }

  loadAnomalies(): void {
    this.http.get<AnomalyAlert[]>('http://localhost:5000/api/anomalies').subscribe(
      data => {
        this.anomalies = data;
      }
    );
  }

  loadStats(): void {
    this.http.get<AnomalyStats>('http://localhost:5000/api/anomalies/stats').subscribe(
      data => {
        this.stats = data;
      }
    );
  }

  resolveAnomaly(anomaly: AnomalyAlert): void {
    this.http.put(`http://localhost:5000/api/anomalies/${anomaly.id}/resolve`, {}).subscribe(
      () => {
        anomaly.isResolved = true;
        this.loadStats();
      }
    );
  }

  getScoreColor(score: number): string {
    if (score >= 0.8) return '#f44336';
    if (score >= 0.6) return '#ff9800';
    return '#fbc02d';
  }

  getTypeIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'UserBehavior': 'person',
      'DataAccess': 'folder_open',
      'LoginPattern': 'login',
      'NetworkActivity': 'wifi'
    };
    return icons[type] || 'warning';
  }

  setupChart(): void {
    this.chartOptions = {
      responsive: true,
      plugins: {
        legend: {
          display: true,
          position: 'bottom'
        }
      }
    };
  }

  updateChart(data: AnomalyAlert[]): void {
    const last24Hours = data.filter(a => {
      const detectedAt = new Date(a.detectedAt);
      const now = new Date();
      return (now.getTime() - detectedAt.getTime()) < 24 * 60 * 60 * 1000;
    });

    const hourlyCount = new Array(24).fill(0);
    last24Hours.forEach(a => {
      const hour = new Date(a.detectedAt).getHours();
      hourlyCount[hour]++;
    });

    this.chartData = [{
      data: hourlyCount,
      label: 'Anomalies Detected'
    }];
  }
}