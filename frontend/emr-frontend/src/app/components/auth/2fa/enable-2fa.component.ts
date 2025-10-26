// enable-2fa.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TwoFactorService, Enable2FAResponse } from '../../services/two-factor.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
  selector: 'app-enable-2fa',
  templateUrl: './enable-2fa.component.html',
  styleUrls: ['./enable-2fa.component.css']
})
export class Enable2FAComponent implements OnInit {
  step = 1;
  qrCodeData: Enable2FAResponse | null = null;
  verifyForm: FormGroup;
  loading = false;
  backupCodesDownloaded = false;

  constructor(
    private fb: FormBuilder,
    private twoFactorService: TwoFactorService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.verifyForm = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  ngOnInit(): void {
    this.enable2FA();
  }

  enable2FA(): void {
    this.loading = true;
    this.twoFactorService.enable2FA().subscribe(
      data => {
        this.qrCodeData = data;
        this.step = 2;
        this.loading = false;
      },
      error => {
        this.snackBar.open('Lỗi khi tạo mã QR', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    );
  }

  nextStep(): void {
    if (this.step < 3) {
      this.step++;
    }
  }

  verify(): void {
    if (this.verifyForm.valid) {
      this.loading = true;
      this.twoFactorService.verify2FA(this.verifyForm.value.code).subscribe(
        () => {
          this.snackBar.open('Đã bật xác thực 2 bước thành công!', 'Đóng', { duration: 3000 });
          this.step = 3;
          this.loading = false;
        },
        error => {
          this.snackBar.open('Mã xác thực không đúng', 'Đóng', { duration: 3000 });
          this.loading = false;
        }
      );
    }
  }

  downloadBackupCodes(): void {
    if (!this.qrCodeData) return;

    const content = this.qrCodeData.backupCodes.join('\n');
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'backup-codes.txt';
    link.click();
    window.URL.revokeObjectURL(url);
    
    this.backupCodesDownloaded = true;
  }

  copyBackupCodes(): void {
    if (!this.qrCodeData) return;

    const content = this.qrCodeData.backupCodes.join('\n');
    navigator.clipboard.writeText(content).then(() => {
      this.snackBar.open('Đã sao chép mã dự phòng', 'Đóng', { duration: 2000 });
    });
  }

  finish(): void {
    this.router.navigate(['/profile']);
  }
}