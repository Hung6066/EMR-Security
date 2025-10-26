import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { UebaService, UebaAlert, UebaMetrics } from '../../../services/ueba.service';
import { UebaAlertDetailDialogComponent } from '../dialogs/ueba-alert-detail-dialog/ueba-alert-detail-dialog.component';

@Component({
  selector: 'app-ueba-dashboard',
  templateUrl: './ueba-dashboard.component.html',
  styleUrls: ['./ueba-dashboard.component.css']
})
export class UebaDashboardComponent implements OnInit {
  alerts: UebaAlert[] = [];
  metrics: UebaMetrics | null = null;
  filterForm: FormGroup;
  loading = true;

  displayedColumns = ['deviationScore', 'userName', 'alertType', 'description', 'detectedAt', 'status', 'actions'];

  constructor(
    private uebaService: UebaService,
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 7);
    this.filterForm = this.fb.group({
      range: this.fb.group({
        from: [yesterday],
        to: [new Date()]
      })
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    const { from, to } = this.filterForm.get('range')?.value;
    if (!from || !to) return;

    const fromDate = from.toISOString();
    const toDate = to.toISOString();

    this.uebaService.getAlerts(fromDate, toDate).subscribe(data => {
      this.alerts = data;
      this.loading = false;
    });

    this.uebaService.getMetrics(fromDate, toDate).subscribe(data => {
      this.metrics = data;
    });
  }

  viewDetails(alert: UebaAlert): void {
    const dialogRef = this.dialog.open(UebaAlertDetailDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      data: alert
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData(); // Tải lại nếu có thay đổi
      }
    });
  }

  getScoreColor(score: number): string {
    if (score >= 0.8) return 'warn';
    if (score >= 0.6) return 'accent';
    return 'primary';
  }

  getScoreLabel(score: number): string {
    if (score >= 0.8) return 'Critical';
    if (score >= 0.6) return 'High';
    if (score >= 0.4) return 'Medium';
    return 'Low';
  }
}