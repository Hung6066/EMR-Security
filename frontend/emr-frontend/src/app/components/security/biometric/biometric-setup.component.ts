// biometric-setup.component.ts
import { Component, OnInit } from '@angular/core';
import { WebAuthnService } from '../../services/webauthn.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
  selector: 'app-biometric-setup',
  templateUrl: './biometric-setup.component.html',
  styleUrls: ['./biometric-setup.component.css']
})
export class BiometricSetupComponent implements OnInit {
  isSupported = false;
  loading = false;
  credentials: any[] = [];
  currentStep = 1;

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
    this.webAuthnService.getCredentials().subscribe(
      data => {
        this.credentials = data;
      },
      error => console.error('Error loading credentials:', error)
    );
  }

  async registerBiometric(): Promise<void> {
    this.loading = true;
    this.currentStep = 2;

    try {
      await this.webAuthnService.registerCredential().toPromise();
      this.currentStep = 3;
      this.snackBar.open('Xác thực sinh trắc học đã được thiết lập!', 'Đóng', { duration: 3000 });
      this.loadCredentials();
    } catch (error: any) {
      console.error('Registration error:', error);
      this.snackBar.open(
        error.message || 'Không thể thiết lập xác thực sinh trắc học',
        'Đóng',
        { duration: 5000 }
      );
      this.currentStep = 1;
    } finally {
      this.loading = false;
    }
  }

  revokeCredential(credential: any): void {
    if (confirm(`Bạn có chắc muốn xóa thiết bị "${credential.deviceName}"?`)) {
      this.webAuthnService.revokeCredential(credential.id).subscribe(
        () => {
          this.snackBar.open('Đã xóa thiết bị', 'Đóng', { duration: 3000 });
          this.loadCredentials();
        },
        error => {
          this.snackBar.open('Lỗi khi xóa thiết bị', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  finish(): void {
    this.router.navigate(['/security']);
  }
}