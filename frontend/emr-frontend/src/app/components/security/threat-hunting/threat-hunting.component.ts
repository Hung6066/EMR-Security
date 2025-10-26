// threat-hunting.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ThreatHuntingService, SuspiciousActivity, ThreatIndicator } from '../../services/threat-hunting.service';
import { CreateThreatQueryDialogComponent } from './create-threat-query-dialog.component';

@Component({
  selector: 'app-threat-hunting',
  templateUrl: './threat-hunting.component.html',
  styleUrls: ['./threat-hunting.component.css']
})
export class ThreatHuntingComponent implements OnInit {
  huntForm: FormGroup;
  suspiciousActivities: SuspiciousActivity[] = [];
  indicators: ThreatIndicator[] = [];
  queries: any[] = [];
  
  hunting = false;
  
  displayedColumnsActivities = ['threatScore', 'activityType', 'description', 'userName', 'timestamp', 'indicators'];
  displayedColumnsIndicators = ['indicatorType', 'value', 'severity', 'source', 'matchCount', 'actions'];

  activityTypes = [
    'UnusualLogin',
    'DataExfiltration',
    'PrivilegeEscalation',
    'UnauthorizedAccess',
    'BruteForce'
  ];

  constructor(
    private fb: FormBuilder,
    private threatHuntingService: ThreatHuntingService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.huntForm = this.fb.group({
      startDate: [new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), Validators.required],
      endDate: [new Date(), Validators.required],
      activityTypes: [[]],
      userId: [null],
      ipAddress: ['']
    });
  }

  ngOnInit(): void {
    this.loadQueries();
    this.loadIndicators();
  }

  loadQueries(): void {
    this.threatHuntingService.getQueries().subscribe(
      data => {
        this.queries = data;
      },
      error => console.error('Error loading queries:', error)
    );
  }

  loadIndicators(): void {
    this.threatHuntingService.getIndicators().subscribe(
      data => {
        this.indicators = data;
      },
      error => console.error('Error loading indicators:', error)
    );
  }

  hunt(): void {
    if (this.huntForm.valid) {
      this.hunting = true;
      
      this.threatHuntingService.huntSuspiciousActivities(this.huntForm.value).subscribe(
        data => {
          this.suspiciousActivities = data;
          this.hunting = false;
          
          if (data.length > 0) {
            this.snackBar.open(
              `Phát hiện ${data.length} hoạt động đáng ngờ!`,
              'Đóng',
              { duration: 5000, panelClass: 'warning-snackbar' }
            );
          } else {
            this.snackBar.open('Không phát hiện mối đe dọa', 'Đóng', { duration: 3000 });
          }
        },
        error => {
          this.hunting = false;
          this.snackBar.open('Lỗi khi hunting', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  createQuery(): void {
    const dialogRef = this.dialog.open(CreateThreatQueryDialogComponent, {
      width: '700px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadQueries();
      }
    });
  }

  executeQuery(query: any): void {
    this.threatHuntingService.executeQuery(query.id).subscribe(
      result => {
        this.snackBar.open(
          `Query executed: ${result.matchCount} matches found`,
          'Đóng',
          { duration: 3000 }
        );
      },
      error => {
        this.snackBar.open('Lỗi khi execute query', 'Đóng', { duration: 3000 });
      }
    );
  }

  addIndicator(): void {
    // Open dialog to add new indicator
  }

  getThreatScoreColor(score: number): string {
    if (score >= 0.8) return '#f44336';
    if (score >= 0.6) return '#ff9800';
    return '#fbc02d';
  }

  getSeverityColor(severity: string): string {
    const colors: { [key: string]: string } = {
      'Critical': 'warn',
      'High': 'accent',
      'Medium': 'primary',
      'Low': ''
    };
    return colors[severity] || '';
  }
}