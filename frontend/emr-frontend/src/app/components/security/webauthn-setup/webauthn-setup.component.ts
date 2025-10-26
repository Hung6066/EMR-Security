import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { WebAuthnService, WebAuthnCredential } from '../../../services/webauthn.service'; // Tạo service này

@Component({
  selector: 'app-webauthn-setup',
  templateUrl: './webauthn-setup.component.html',
  styleUrls: ['./webauthn-setup.component.css']
})
export class WebAuthnSetupComponent implements OnInit {
  isSupported = false;
  loading = false;
  credentials: WebAuthnCredential[] = [];
  currentStep = 1;

  displayedColumns = ['deviceName', 'createdAt', 'lastUsedAt', 'actions'];

  constructor(
    private webAuthnService: WebAuthnService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isSupported = this.webAuthnService.isWebAuthnSupported();
    if (this.isSupported) {
      this.loadCredentials();
    }
  }

  loadCredentials(): void {
    this.webAuthnService.getCredentials().subscribe({
      next: data => this.credentials = data,
      error: err => console.error('Error loading credentials:', err)
    });
  }

  async registerBiometric(): Promise<void> {
    this.loading = true;
    this.currentStep = 2; // Chuyển sang bước "Đang đăng ký"

    this.webAuthnService.registerCredential().subscribe({
      next: () => {
        this.currentStep = 3; // Chuyển sang bước "Thành công"
        this.snackBar.open('Xác thực sinh trắc học đã được thiết lập!', 'Đóng', { duration: 3000, panelClass: 'success-snackbar' });
        this.loadCredentials();
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Registration error:', error);
        this.snackBar.open(
          error.message || 'Không thể thiết lập. Vui lòng thử lại.',
          'Đóng',
          { duration: 5000, panelClass: 'error-snackbar' }
        );
        this.currentStep = 1; // Quay lại bước đầu
        this.loading = false;
      }
    });
  }

  revokeCredential(credential: WebAuthnCredential): void {
    if (confirm(`Bạn có chắc muốn xóa thiết bị "${credential.deviceName}"?`)) {
      this.webAuthnService.revokeCredential(credential.id).subscribe({
        next: () => {
          this.snackBar.open('Đã xóa thiết bị', 'Đóng', { duration: 3000 });
          this.loadCredentials();
        },
        error: () => this.snackBar.open('Lỗi khi xóa thiết bị', 'Đóng', { duration: 3000, panelClass: 'error-snackbar' })
      });
    }
  }

  finish(): void {
    this.router.navigate(['/security']);
  }

  getDeviceIcon(deviceName: string): string {
    const name = deviceName.toLowerCase();
    if (name.includes('windows')) return 'laptop_windows';
    if (name.includes('mac') || name.includes('iphone')) return 'laptop_mac';
    if (name.includes('android')) return 'phone_android';
    return 'security'; // Default icon
  }
}