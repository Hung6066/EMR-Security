// api-key-display-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiKeyResponse } from '../../services/api-key.service';

@Component({
  selector: 'app-api-key-display-dialog',
  templateUrl: './api-key-display-dialog.component.html',
  styleUrls: ['./api-key-display-dialog.component.css']
})
export class ApiKeyDisplayDialogComponent {
  copied = false;
  confirmed = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ApiKeyResponse,
    public dialogRef: MatDialogRef<ApiKeyDisplayDialogComponent>,
    private snackBar: MatSnackBar
  ) {}

  copyToClipboard(): void {
    navigator.clipboard.writeText(this.data.apiKey).then(() => {
      this.copied = true;
      this.snackBar.open('Đã sao chép API key', 'Đóng', { duration: 2000 });
    });
  }

  downloadAsFile(): void {
    const content = `API Key Information
Name: ${this.data.name}
API Key: ${this.data.apiKey}
Created: ${this.data.createdAt}
Expires: ${this.data.expiresAt || 'Never'}

Keep this key secure. It will not be shown again.`;

    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `api-key-${this.data.keyPrefix}.txt`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  close(): void {
    if (this.confirmed) {
      this.dialogRef.close();
    } else {
      alert('Vui lòng xác nhận bạn đã lưu API key trước khi đóng.');
    }
  }
}