// security-settings.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SecurityService } from '../../services/security.service';
import { TwoFactorService } from '../../services/two-factor.service';
import { ChangePasswordDialogComponent } from './change-password-dialog.component';
import { DisableTwoFactorDialogComponent } from './disable-2fa-dialog.component';

export interface Session {
  id: number;
  deviceInfo: string;
  ipAddress: string;
  location: string;
  createdAt: Date;
  lastActivityAt: Date;
  isActive: boolean;
}

export interface LoginHistory {
  id: number;
  ipAddress: string;
  userAgent: string;
  location: string;
  isSuccessful: boolean;
  failureReason: string;
  attemptedAt: Date;
}

@Component({
  selector: 'app-security-settings',
  templateUrl: './security-settings.component.html',
  styleUrls: ['./security-settings.component.css']
})
export class SecuritySettingsComponent implements OnInit {
  sessions: Session[] = [];
  loginHistory: LoginHistory[] = [];
  twoFactorEnabled = false;
  loading = false;

  displayedColumnsSession = ['deviceInfo', 'ipAddress', 'lastActivityAt', 'actions'];
  displayedColumnsHistory = ['attemptedAt', 'ipAddress', 'location', 'status'];

  constructor(
    private securityService: SecurityService,
    private twoFactorService: TwoFactorService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadSessions();
    this.loadLoginHistory();
    this.check2FAStatus();
  }

  loadSessions(): void {
    this.securityService.getActiveSessions().subscribe(
      data => {
        this.sessions = data;
      },
      error => console.error('Error loading sessions:', error)
    );
  }

  loadLoginHistory(): void {
    this.securityService.getLoginHistory().subscribe(
      data => {
        this.loginHistory = data;
      },
      error => console.error('Error loading login history:', error)
    );
  }

  check2FAStatus(): void {
    // Implement check from current user data
    // this.twoFactorEnabled = ...
  }

  revokeSession(sessionId: number): void {
    if (confirm('Bạn có chắc muốn thu hồi phiên này?')) {
      this.securityService.revokeSession(sessionId).subscribe(
        () => {
          this.snackBar.open('Đã thu hồi phiên', 'Đóng', { duration: 3000 });
          this.loadSessions();
        },
        error => {
          this.snackBar.open('Lỗi khi thu hồi phiên', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  revokeAllSessions(): void {
    if (confirm('Bạn có chắc muốn thu hồi tất cả phiên đăng nhập khác?')) {
      this.securityService.revokeAllSessions().subscribe(
        () => {
          this.snackBar.open('Đã thu hồi tất cả phiên', 'Đóng', { duration: 3000 });
          this.loadSessions();
        },
        error => {
          this.snackBar.open('Lỗi khi thu hồi phiên', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  changePassword(): void {
    const dialogRef = this.dialog.open(ChangePasswordDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Đã đổi mật khẩu thành công', 'Đóng', { duration: 3000 });
      }
    });
  }

  enable2FA(): void {
    this.router.navigate(['/enable-2fa']);
  }

  disable2FA(): void {
    const dialogRef = this.dialog.open(DisableTwoFactorDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.twoFactorEnabled = false;
        this.snackBar.open('Đã tắt xác thực 2 bước', 'Đóng', { duration: 3000 });
      }
    });
  }

  regenerateBackupCodes(): void {
    if (confirm('Tạo mới mã dự phòng sẽ vô hiệu hóa tất cả mã cũ. Tiếp tục?')) {
      this.twoFactorService.regenerateBackupCodes().subscribe(
        codes => {
          // Show backup codes dialog
          this.showBackupCodesDialog(codes);
        },
        error => {
          this.snackBar.open('Lỗi khi tạo mã dự phòng', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  showBackupCodesDialog(codes: string[]): void {
    // Implement backup codes display dialog
  }

  getStatusIcon(isSuccessful: boolean): string {
    return isSuccessful ? 'check_circle' : 'cancel';
  }

  getStatusColor(isSuccessful: boolean): string {
    return isSuccessful ? 'primary' : 'warn';
  }
}