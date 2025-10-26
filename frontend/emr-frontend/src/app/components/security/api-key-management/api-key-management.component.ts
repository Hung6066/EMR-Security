// api-key-management.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiKeyService, ApiKey, ApiKeyResponse } from '../../services/api-key.service';
import { CreateApiKeyDialogComponent } from './dialogs/create-api-key-dialog.component';
import { ApiKeyDisplayDialogComponent } from './api-key-display-dialog.component';

@Component({
  selector: 'app-api-key-management',
  templateUrl: './api-key-management.component.html',
  styleUrls: ['./api-key-management.component.css']
})
export class ApiKeyManagementComponent implements OnInit {
  apiKeys: ApiKey[] = [];
  displayedColumns = ['name', 'keyPrefix', 'createdAt', 'expiresAt', 'lastUsedAt', 'status', 'actions'];

  constructor(
    private apiKeyService: ApiKeyService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadApiKeys();
  }

  loadApiKeys(): void {
    this.apiKeyService.getApiKeys().subscribe(
      data => {
        this.apiKeys = data;
      },
      error => console.error('Error loading API keys:', error)
    );
  }

  createApiKey(): void {
    const dialogRef = this.dialog.open(CreateApiKeyDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe((result: ApiKeyResponse) => {
      if (result) {
        this.loadApiKeys();
        
        // Show API key (only time it's displayed)
        this.dialog.open(ApiKeyDisplayDialogComponent, {
          width: '600px',
          data: result,
          disableClose: true
        });
      }
    });
  }

  revokeApiKey(key: ApiKey): void {
    if (confirm(`Bạn có chắc muốn thu hồi API key "${key.name}"?`)) {
      this.apiKeyService.revokeApiKey(key.id).subscribe(
        () => {
          this.snackBar.open('Đã thu hồi API key', 'Đóng', { duration: 3000 });
          this.loadApiKeys();
        },
        error => {
          this.snackBar.open('Lỗi khi thu hồi API key', 'Đóng', { duration: 3000 });
        }
      );
    }
  }

  getStatusColor(key: ApiKey): string {
    if (key.isRevoked) return 'warn';
    if (key.expiresAt && new Date(key.expiresAt) < new Date()) return 'warn';
    return 'primary';
  }

  getStatusText(key: ApiKey): string {
    if (key.isRevoked) return 'Đã thu hồi';
    if (key.expiresAt && new Date(key.expiresAt) < new Date()) return 'Đã hết hạn';
    return 'Hoạt động';
  }
}