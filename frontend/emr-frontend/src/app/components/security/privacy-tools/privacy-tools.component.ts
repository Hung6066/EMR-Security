// privacy-tools.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PrivacyService } from '../../services/privacy.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-privacy-tools',
  templateUrl: './privacy-tools.component.html',
  styleUrls: ['./privacy-tools.component.css']
})
export class PrivacyToolsComponent {
  anonymizeForm: FormGroup;
  tokenizeForm: FormGroup;
  syntheticForm: FormGroup;
  
  anonymizationMethods = ['Anonymization', 'Pseudonymization', 'Masking'];
  dataTypes = ['Patient', 'MedicalRecord', 'User'];
  
  result: any = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private privacyService: PrivacyService,
    private snackBar: MatSnackBar
  ) {
    this.anonymizeForm = this.fb.group({
      dataType: ['Patient', Validators.required],
      method: ['Anonymization', Validators.required],
      inputData: ['', Validators.required]
    });

    this.tokenizeForm = this.fb.group({
      data: ['', Validators.required],
      tokenType: ['SSN', Validators.required]
    });

    this.syntheticForm = this.fb.group({
      sourceDataType: ['Patient', Validators.required],
      recordCount: [100, [Validators.required, Validators.min(1), Validators.max(10000)]],
      preservePatterns: [true]
    });
  }

  anonymize(): void {
    if (this.anonymizeForm.valid) {
      this.loading = true;
      this.result = null;

      try {
        const inputData = JSON.parse(this.anonymizeForm.value.inputData);
        const config = {
          dataType: this.anonymizeForm.value.dataType,
          method: this.anonymizeForm.value.method,
          fields: Object.keys(inputData)
        };

        this.privacyService.anonymizeData(inputData, config).subscribe(
          data => {
            this.result = {
              type: 'anonymized',
              data: data
            };
            this.loading = false;
          },
          error => {
            this.snackBar.open('Lỗi khi ẩn danh hóa dữ liệu', 'Đóng', { duration: 3000 });
            this.loading = false;
          }
        );
      } catch (e) {
        this.snackBar.open('Dữ liệu đầu vào không hợp lệ (JSON)', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    }
  }

  tokenize(): void {
    if (this.tokenizeForm.valid) {
      this.loading = true;
      
      this.privacyService.tokenizeData(
        this.tokenizeForm.value.data,
        this.tokenizeForm.value.tokenType
      ).subscribe(
        token => {
          this.result = {
            type: 'token',
            original: this.tokenizeForm.value.data,
            token: token
          };
          this.loading = false;
        },
        error => {
          this.snackBar.open('Lỗi khi tạo token', 'Đóng', { duration: 3000 });
          this.loading = false;
        }
      );
    }
  }

  generateSynthetic(): void {
    if (this.syntheticForm.valid) {
      this.loading = true;
      
      this.privacyService.generateSyntheticData(this.syntheticForm.value).subscribe(
        data => {
          this.result = {
            type: 'synthetic',
            data: data,
            count: data.length
          };
          this.loading = false;
        },
        error => {
          this.snackBar.open('Lỗi khi tạo dữ liệu tổng hợp', 'Đóng', { duration: 3000 });
          this.loading = false;
        }
      );
    }
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.snackBar.open('Đã sao chép', 'Đóng', { duration: 2000 });
    });
  }

  downloadResult(): void {
    if (!this.result) return;

    const dataStr = JSON.stringify(this.result.data, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `privacy-result-${Date.now()}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}