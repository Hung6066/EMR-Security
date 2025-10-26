// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { ReportService, DashboardStats } from '../../services/report.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats | null = null;
  loading = true;

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.loadDashboardStats();
  }

  loadDashboardStats(): void {
    this.reportService.getDashboardStats().subscribe(
      data => {
        this.stats = data;
        this.loading = false;
      },
      error => {
        console.error('Error loading dashboard stats:', error);
        this.loading = false;
      }
    );
  }
}