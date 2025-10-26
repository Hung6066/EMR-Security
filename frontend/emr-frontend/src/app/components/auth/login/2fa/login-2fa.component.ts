// login-2fa.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login-2fa',
  templateUrl: './login-2fa.component.html',
  styleUrls: ['./login-2fa.component.css']
})
export class Login2FAComponent implements OnInit {
  twoFactorForm: FormGroup;
  loading = false;
  error = '';
  email = '';
  password = '';
  useBackupCode = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.twoFactorForm = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6,8}$/)]]
    });
  }

  ngOnInit(): void {
    // Get email and password from route state
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras.state as any;
    
    if (state) {
      this.email = state.email;
      this.password = state.password;
    } else {
      this.router.navigate(['/login']);
    }
  }

 onSubmit(): void {
  if (this.loginForm.valid) {
    this.loading = true;
    this.error = '';

    this.authService.login(this.loginForm.value).subscribe(
      response => {
        if (response.requiresTwoFactor) {
          // Navigate to 2FA page
          this.router.navigate(['/login-2fa'], {
            state: {
              email: this.loginForm.value.email,
              password: this.loginForm.value.password
            }
          });
        } else {
          this.router.navigate([this.returnUrl]);
        }
        this.loading = false;
      },
      error => {
        this.error = error.error?.message || 'Đăng nhập thất bại';
        this.loading = false;
      }
    );
  }
}

  toggleBackupCode(): void {
    this.useBackupCode = !this.useBackupCode;
    const codeControl = this.twoFactorForm.get('code');
    
    if (this.useBackupCode) {
      codeControl?.setValidators([Validators.required, Validators.pattern(/^\d{8}$/)]);
    } else {
      codeControl?.setValidators([Validators.required, Validators.pattern(/^\d{6}$/)]);
    }
    
    codeControl?.updateValueAndValidity();
    codeControl?.setValue('');
  }

  getDeviceInfo(): string {
    const ua = navigator.userAgent;
    if (/mobile/i.test(ua)) return 'Mobile Device';
    if (/tablet/i.test(ua)) return 'Tablet';
    return 'Desktop';
  }

  
}