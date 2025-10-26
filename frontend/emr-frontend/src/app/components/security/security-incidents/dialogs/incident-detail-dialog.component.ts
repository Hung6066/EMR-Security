// incident-detail-dialog.component.ts
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormControl } from '@angular/forms';
import { SecurityIncidentService, SecurityIncident } from '../../services/security-incident.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-incident-detail-dialog',
  templateUrl: './incident-detail-dialog.component.html',
  styleUrls: ['./incident-detail-dialog.component.css']
})
export class IncidentDetailDialogComponent implements OnInit {
  incident: SecurityIncident;
  commentControl = new FormControl('');
  
  statuses = ['New', 'Investigating', 'Contained', 'Resolved', 'Closed'];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SecurityIncident,
    private dialogRef: MatDialogRef<IncidentDetailDialogComponent>,
    private incidentService: SecurityIncidentService,
    private snackBar: MatSnackBar
  ) {
    this.incident = data;
  }

  ngOnInit(): void {
    this.loadIncidentDetails();
  }

  loadIncidentDetails(): void {
    this.incidentService.getIncidentById(this.incident.id).subscribe(
      data => {
        this.incident = data;
      },
      error => console.error('Error loading incident details:', error)
    );
  }

  updateStatus(status: string): void {
    const notes = prompt(`Cập nhật trạng thái thành "${status}". Ghi chú:`);
    if (notes !== null) {
      this.incidentService.updateStatus(this.incident.id, status, notes).subscribe(
        () => {
          this.snackBar.open('Đã cập nhật trạng thái', 'Đóng', { duration: 3000 });
          this.loadIncidentDetails();
        },
        error => {
          this.snackBar.open('Lỗi khi cập nhật', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  addComment(): void {
    if (this.commentControl.value?.trim()) {
      this.incidentService.addComment(this.incident.id, this.commentControl.value).subscribe(
        () => {
          this.commentControl.setValue('');
          this.loadIncidentDetails();
          this.snackBar.open('Đã thêm comment', 'Đóng', { duration: 2000 });
        },
        error => {
          this.snackBar.open('Lỗi khi thêm comment', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  close(): void {
    this.dialogRef.close(true);
  }

  getSeverityClass(): string {
    return `severity-${this.incident.severity.toLowerCase()}`;
  }

  getTimelineIcon(action: any): string {
    const icons: { [key: string]: string } = {
      'Created': 'add_circle',
      'Status Change': 'update',
      'Assigned': 'person_add',
      'BlockIP': 'block',
      'DisableUser': 'person_off',
      'RevokeAllSessions': 'exit_to_app',
      'NotifyAdmin': 'notifications'
    };
    return icons[action.actionType] || 'info';
  }
}