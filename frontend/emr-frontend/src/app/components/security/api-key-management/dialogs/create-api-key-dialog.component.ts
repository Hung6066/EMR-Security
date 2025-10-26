// create-api-key-dialog.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { ApiKeyService } from '../../services/api-key.service';

@Component({
  selector: 'app-create-api-key-dialog',
  templateUrl: './create-api-key-dialog.component.html',
  styleUrls: ['./create-api-key-dialog.component.css']
})
export class CreateApiKeyDialogComponent {
  apiKeyForm: FormGroup;
  loading = false;
  error = '';

  availableScopes = [
    { value: 'read:patients', label: 'Đọc thông tin bệnh nhân' },
    { value: 'write:patients', label: 'Ghi thông tin bệnh nhân' },
    { value: 'read:records', label: 'Đọc bệnh án' },
    { value: 'write:records', label: 'Ghi bệnh án' },
    { value: 'read:appointments', label: 'Đọc lịch hẹn' },
    { value: 'write:appointments', label: 'Ghi lịch hẹn' }
  ];

  constructor(
    private fb: FormBuilder,
    private apiKeyService: ApiKeyService,
    public dialogRef: MatDialogRef<CreateApiKeyDialogComponent>
  ) {
    this.apiKeyForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      expiresAt: [null],
      ipWhitelist: [''],
      scopes: [[]],
      rateLimitPerMinute: [60, [Validators.min(1), Validators.max(1000)]]
    });
  }

  onSubmit(): void {
    if (this.apiKeyForm.valid) {
      this.loading = true;
      this.error = '';

      this.apiKeyService.createApiKey(this.apiKeyForm.value).subscribe(
        result => {
          this.dialogRef.close(result);
        },
        error => {
          this.error = error.error?.message || 'Lỗi khi tạo API key';
          this.loading = false;
        }
      );
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}