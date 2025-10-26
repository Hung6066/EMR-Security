import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UebaService, UebaAlert } from '../../../../services/ueba.service';

@Component({
  selector: 'app-ueba-alert-detail-dialog',
  templateUrl: './ueba-alert-detail-dialog.component.html',
  styleUrls: ['./ueba-alert-detail-dialog.component.css']
})
export class UebaAlertDetailDialogComponent {
  alert: UebaAlert;
  contextData: any;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: UebaAlert,
    public dialogRef: MatDialogRef<UebaAlertDetailDialogComponent>,
    private uebaService: UebaService,
    private snackBar: MatSnackBar
  ) {
    this.alert = data;
    try {
      this.contextData = JSON.parse(this.alert.context);
    } catch (e) {
      this.contextData = { raw: this.alert.context };
      console.error('Could not parse alert context:', e);
    }
  }

  updateStatus(status: string): void {
    this.uebaService.updateAlertStatus(this.alert.id, status).subscribe({
      next: () => {
        this.snackBar.open(`Alert status updated to "${status}"`, 'OK', { duration: 3000 });
        this.alert.status = status;
      },
      error: () => this.snackBar.open('Failed to update status', 'OK', { duration: 3000, panelClass: 'error-snackbar' })
    });
  }

  close(): void {
    this.dialogRef.close(true);
  }

  getScoreColorClass(): string {
    const score = this.alert.deviationScore;
    if (score >= 0.8) return 'critical';
    if (score >= 0.6) return 'high';
    return 'medium';
  }
}