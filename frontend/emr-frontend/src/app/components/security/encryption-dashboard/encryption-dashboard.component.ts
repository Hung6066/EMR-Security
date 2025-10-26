// encryption-dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EncryptionService, EncryptionKey, VaultEntry, EncryptionMetrics } from '../../services/encryption.service';
import { CreateKeyDialogComponent } from './create-key-dialog.component';
import { VaultEntryDialogComponent } from './vault-entry-dialog.component';

@Component({
  selector: 'app-encryption-dashboard',
  templateUrl: './encryption-dashboard.component.html',
  styleUrls: ['./encryption-dashboard.component.css']
})
export class EncryptionDashboardComponent implements OnInit {
  keys: EncryptionKey[] = [];
  vaultEntries: VaultEntry[] = [];
  metrics: EncryptionMetrics | null = null;
  
  displayedColumnsKeys = ['keyName', 'keyType', 'purpose', 'keySize', 'status', 'expiresAt', 'actions'];
  displayedColumnsVault = ['keyName', 'description', 'createdAt', 'expiresAt', 'actions'];

  selectedTab = 0;

  constructor(
    private encryptionService: EncryptionService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadKeys();
    this.loadVaultEntries();
    this.loadMetrics();
  }

  loadKeys(): void {
    this.encryptionService.getKeys().subscribe(
      data => {
        this.keys = data;
      },
      error => console.error('Error loading keys:', error)
    );
  }

  loadVaultEntries(): void {
    this.encryptionService.getVaultEntries().subscribe(
      data => {
        this.vaultEntries = data;
      },
      error => console.error('Error loading vault entries:', error)
    );
  }

  loadMetrics(): void {
    this.encryptionService.getMetrics().subscribe(
      data => {
        this.metrics = data;
      },
      error => console.error('Error loading metrics:', error)
    );
  }

  createKey(): void {
    const dialogRef = this.dialog.open(CreateKeyDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadKeys();
        this.loadMetrics();
      }
    });
  }

  rotateKey(key: EncryptionKey): void {
    if (confirm(`Bạn có chắc muốn xoay vòng khóa "${key.keyName}"?\nĐiều này sẽ tạo khóa mới và mã hóa lại dữ liệu.`)) {
      this.encryptionService.rotateKey(key.id).subscribe(
        () => {
          this.snackBar.open('Đã xoay vòng khóa thành công', 'Đóng', { duration: 3000 });
          this.loadKeys();
        },
        error => {
          this.snackBar.open('Lỗi khi xoay vòng khóa', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  storeInVault(): void {
    const dialogRef = this.dialog.open(VaultEntryDialogComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadVaultEntries();
      }
    });
  }

  viewVaultEntry(entry: VaultEntry): void {
    this.encryptionService.getFromVault(entry.keyName).subscribe(
      value => {
        const dialogRef = this.dialog.open(VaultEntryDialogComponent, {
          width: '600px',
          data: { mode: 'view', entry, value }
        });
      },
      error => {
        this.snackBar.open('Lỗi khi lấy dữ liệu từ vault', 'Đóng', { duration: 3000 });
      }
    );
  }

  deleteVaultEntry(entry: VaultEntry): void {
    if (confirm(`Bạn có chắc muốn xóa "${entry.keyName}" từ vault?`)) {
      this.encryptionService.deleteFromVault(entry.id).subscribe(
        () => {
          this.snackBar.open('Đã xóa khỏi vault', 'Đóng', { duration: 3000 });
          this.loadVaultEntries();
        },
        error => {
          this.snackBar.open('Lỗi khi xóa', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  getKeyStatus(key: EncryptionKey): string {
    if (key.isRotated) return 'Rotated';
    if (!key.isActive) return 'Inactive';
    
    const daysUntilExpiry = this.getDaysUntilExpiry(key.expiresAt);
    if (daysUntilExpiry < 0) return 'Expired';
    if (daysUntilExpiry < 30) return 'Expiring Soon';
    
    return 'Active';
  }

  getStatusColor(key: EncryptionKey): string {
    const status = this.getKeyStatus(key);
    const colors: { [key: string]: string } = {
      'Active': 'primary',
      'Expiring Soon': 'accent',
      'Expired': 'warn',
      'Rotated': '',
      'Inactive': ''
    };
    return colors[status] || '';
  }

  getDaysUntilExpiry(expiresAt: Date): number {
    const now = new Date();
    const expiry = new Date(expiresAt);
    const diff = expiry.getTime() - now.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  isExpiringSoon(key: EncryptionKey): boolean {
    const days = this.getDaysUntilExpiry(key.expiresAt);
    return days >= 0 && days < 30;
  }
}