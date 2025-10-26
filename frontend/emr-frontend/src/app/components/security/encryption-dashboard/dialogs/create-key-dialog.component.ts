// create-key-dialog.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { EncryptionService } from '../../services/encryption.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-create-key-dialog',
  templateUrl: './create-key-dialog.component.html',
  styleUrls: ['./create-key-dialog.component.css']
})
export class CreateKeyDialogComponent {
  keyForm: FormGroup;
  loading = false;

  keyTypes = ['AES', 'RSA', 'HMAC'];
  purposes = ['DataEncryption', 'Signing', 'TokenEncryption', 'VaultStorage'];
  keySizes: { [key: string]: number[] } = {
    'AES': [128, 192, 256],
    'RSA': [2048, 3072, 4096],
    'HMAC': [256, 384, 512]
  };

  constructor(
    private fb: FormBuilder,
    private encryptionService: EncryptionService,
    private dialogRef: MatDialogRef<CreateKeyDialogComponent>,
    private snackBar: MatSnackBar
  ) {
    this.keyForm = this.fb.group({
      keyName: ['', [Validators.required, Validators.maxLength(100)]],
      keyType: ['AES', Validators.required],
      purpose: ['DataEncryption', Validators.required],
      keySize: [256, Validators.required],
      validityDays: [365, [Validators.required, Validators.min(1), Validators.max(3650)]]
    });

    // Update key size options when type changes
    this.keyForm.get('keyType')?.valueChanges.subscribe(type => {
      const sizes = this.keySizes[type];
      if (sizes) {
        this.keyForm.patchValue({ keySize: sizes[sizes.length - 1] });
      }
    });
  }

  getAvailableKeySizes(): number[] {
    const type = this.keyForm.get('keyType')?.value;
    return this.keySizes[type] || [];
  }

  onSubmit(): void {
    if (this.keyForm.valid) {
      this.loading = true;
      
      this.encryptionService.createKey(this.keyForm.value).subscribe(
        () => {
          this.snackBar.open('Đã tạo khóa thành công', 'Đóng', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error => {
          this.snackBar.open('Lỗi khi tạo khóa', 'Đóng', { duration: 3000 });
          this.loading = false;
        }
      );
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}