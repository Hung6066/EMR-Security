// biometric-login.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { WebAuthnService } from '../../services/webauthn.service';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-biometric-login',
  templateUrl: './biometric-login.component.html',
  styleUrls: ['./biometric-login.component.css']
})
export class BiometricLoginComponent {
  email = '';
  loading = false;

  constructor(
    private webAuthnService: WebAuthnService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  async loginWithBiometric(): Promise<void> {
    if (!this.email) {
      this.snackBar.open('Vui lòng nhập email', 'Đóng', { duration: 3000 });
      return;
    }

    this.loading = true;

    try {
      const response = await this.webAuthnService.loginWithBiometric(this.email).toPromise();
      
      // Store tokens
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      
      this.snackBar.open('Đăng nhập thành công!', 'Đóng', { duration: 2000 });
      this.router.navigate(['/dashboard']);
    } catch (error: any) {
      console.error('Biometric login error:', error);
      this.snackBar.open(
        'Xác thực sinh trắc học thất bại. Vui lòng thử lại.',
        'Đóng',
        { duration: 5000 }
      );
    } finally {
      this.loading = false;
    }
  }
}