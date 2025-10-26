import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SecurityService, TrustedDevice } from '../../../services/security.service'; // Service này đã có ở các phần trước

@Component({
  selector: 'app-trusted-devices',
  templateUrl: './trusted-devices.component.html',
  styleUrls: ['./trusted-devices.component.css']
})
export class TrustedDevicesComponent implements OnInit {
  devices: TrustedDevice[] = [];
  displayedColumns = ['deviceInfo', 'ipAddress', 'lastActivityAt', 'actions'];
  loading = true;

  constructor(
    private securityService: SecurityService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadDevices();
  }

  loadDevices(): void {
    this.loading = true;
    this.securityService.getActiveSessions().subscribe({
      next: data => {
        this.devices = data;
        this.loading = false;
      },
      error: () => {
        this.snackBar.open('Lỗi khi tải danh sách thiết bị', 'Đóng', { duration: 3000, panelClass: 'error-snackbar' });
        this.loading = false;
      }
    });
  }

  revokeDevice(device: TrustedDevice): void {
    if (confirm(`Bạn có chắc muốn đăng xuất khỏi thiết bị "${device.deviceInfo}"?`)) {
      this.securityService.revokeSession(device.id).subscribe({
        next: () => {
          this.snackBar.open('Đã đăng xuất khỏi thiết bị', 'Đóng', { duration: 3000 });
          this.loadDevices();
        },
        error: () => this.snackBar.open('Lỗi khi thực hiện', 'Đóng', { duration: 3000, panelClass: 'error-snackbar' })
      });
    }
  }

  revokeAll(): void {
    if (confirm('Bạn có chắc muốn đăng xuất khỏi tất cả các thiết bị khác?')) {
      this.securityService.revokeAllSessions().subscribe({
        next: () => {
          this.snackBar.open('Đã đăng xuất khỏi tất cả các thiết bị khác', 'Đóng', { duration: 3000 });
          this.loadDevices();
        },
        error: () => this.snackBar.open('Lỗi khi thực hiện', 'Đóng', { duration: 3000, panelClass: 'error-snackbar' })
      });
    }
  }

  getDeviceIcon(userAgent: string): string {
    const ua = userAgent.toLowerCase();
    if (ua.includes('mobile')) return 'phone_android';
    if (ua.includes('tablet')) return 'tablet_mac';
    if (ua.includes('windows')) return 'laptop_windows';
    if (ua.includes('macintosh')) return 'laptop_mac';
    if (ua.includes('linux')) return 'laptop';
    return 'computer';
  }
}