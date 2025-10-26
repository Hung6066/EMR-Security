// src/app/components/security/password-policy/password-policy.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PasswordPolicyService, PasswordPolicy } from '../../../services/password-policy.service';

@Component({
  selector: 'app-password-policy',
  templateUrl: './password-policy.component.html',
  styleUrls: ['./password-policy.component.css']
})
export class PasswordPolicyComponent implements OnInit {
  policyForm: FormGroup;
  loading = true;

  constructor(
    private fb: FormBuilder,
    private policyService: PasswordPolicyService,
    private snackBar: MatSnackBar
  ) {
    this.policyForm = this.fb.group({
      id: [0],
      minLength: [8, [Validators.required, Validators.min(6), Validators.max(32)]],
      requireLowercase: [true],
      requireUppercase: [true],
      requireDigit: [true],
      requireNonAlphanumeric: [false],
      expireDays: [90, [Validators.required, Validators.min(30), Validators.max(365)]],
      passwordHistory: [5, [Validators.required, Validators.min(0), Validators.max(24)]],
      checkPwnedPasswords: [true],
      maxFailedAccessAttempts: [5, [Validators.required, Validators.min(3), Validators.max(20)]],
      lockoutMinutes: [15, [Validators.required, Validators.min(5), Validators.max(1440)]]
    });
  }

  ngOnInit(): void {
    this.loadPolicy();
  }

  loadPolicy(): void {
    this.loading = true;
    this.policyService.getPolicy().subscribe(
      policy => {
        this.policyForm.patchValue(policy);
        this.loading = false;
      },
      error => {
        this.snackBar.open('Lỗi khi tải chính sách', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    );
  }

  savePolicy(): void {
    if (this.policyForm.valid) {
      this.loading = true;
      this.policyService.updatePolicy(this.policyForm.value).subscribe(
        () => {
          this.snackBar.open('Đã cập nhật chính sách mật khẩu', 'Đóng', { duration: 3000 });
          this.loading = false;
        },
        error => {
          this.snackBar.open('Lỗi khi cập nhật chính sách', 'Đóng', { duration: 3000 });
          this.loading = false;
        }
      );
    }
  }
}