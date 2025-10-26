// src/app/components/reports/advanced-reports/advanced-reports.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Chart, registerables } from 'chart.js';
import { AdvancedReportsService, ReportArchive } from '../../../services/advanced-reports.service';

Chart.register(...registerables);

@Component({
  selector: 'app-advanced-reports',
  templateUrl: './advanced-reports.component.html',
  styleUrls: ['./advanced-reports.component.css']
})
export class AdvancedReportsComponent implements OnInit {
  reportForm: FormGroup;
  archive: ReportArchive[] = [];
  exporting = false;
  
  reportTypes = ['Security', 'Usage', 'Clinical'];
  formats = ['PDF', 'CSV'];
  
  displayedColumns = ['title', 'type', 'format', 'generatedAt', 'actions'];

  constructor(
    private fb: FormBuilder,
    private reportService: AdvancedReportsService,
    private snackBar: MatSnackBar
  ) {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);

    this.reportForm = this.fb.group({
      type: ['Security', Validators.required],
      format: ['PDF', Validators.required],
      range: this.fb.group({
        from: [lastMonth, Validators.required],
        to: [yesterday, Validators.required]
      })
    });
  }

  ngOnInit(): void {
    this.loadArchive();
  }

  loadArchive(): void {
    this.reportService.archive().subscribe(data => {
      this.archive = data;
    });
  }

  exportReport(): void {
    if (this.reportForm.valid) {
      this.exporting = true;
      const { type, format, range } = this.reportForm.value;
      const from = new Date(range.from).toISOString().split('T')[0];
      const to = new Date(range.to).toISOString().split('T')[0];

      this.reportService.export(type, from, to, format).subscribe(
        () => {
          this.snackBar.open(`Report "${type}" đang được tạo...`, 'Đóng', { duration: 3000 });
          this.exporting = false;
          // Refresh archive list after a delay
          setTimeout(() => this.loadArchive(), 5000);
        },
        error => {
          this.snackBar.open('Lỗi khi tạo report', 'Đóng', { duration: 3000 });
          this.exporting = false;
        }
      );
    }
  }

  download(item: ReportArchive): void {
    this.reportService.download(item.id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `report_${item.type}_${item.id}.${item.format.toLowerCase()}`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}