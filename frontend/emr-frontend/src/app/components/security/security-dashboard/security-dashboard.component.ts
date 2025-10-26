import { Component } from '@angular/core';

@Component({
  selector: 'app-security-dashboard',
  templateUrl: './security-dashboard.component.html',
  styleUrls: ['./security-dashboard.component.css']
})
export class SecurityDashboardComponent {
  // Dữ liệu cho các widget sẽ được load bởi các component con tương ứng.
  // Component này đóng vai trò là layout chính.

  // Ví dụ các quick link
  quickLinks = [
    { title: 'Chính sách Mật khẩu', icon: 'policy', route: '/security/password-policy' },
    { title: 'Quản lý Thiết bị', icon: 'devices', route: '/security/trusted-devices' },
    { title: 'Xác thực Sinh trắc học', icon: 'fingerprint', route: '/security/webauthn-setup' },
    { title: 'Sự cố Bảo mật', icon: 'security', route: '/security/security-incidents' },
    { title: 'Nhật ký Kiểm toán', icon: 'description', route: '/security/audit-log' },
    { title: 'Threat Hunting', icon: 'search', route: '/security/threat-hunting' },
    { title: 'Blockchain Explorer', icon: 'link', route: '/security/blockchain-explorer' },
  ];

  constructor() { }
}