// login.component.ts
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RecaptchaComponent } from 'ng-recaptcha';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  @ViewChild('captchaRef') captchaRef: RecaptchaComponent;
  
  loginForm: FormGroup;
  loading = false;
  error = '';
  returnUrl = '';
  showCaptcha = false;
  failedAttempts = 0;
  captchaToken: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    
    // Check if should show captcha based on previous attempts
    const attempts = localStorage.getItem('loginAttempts');
    if (attempts && parseInt(attempts) >= 3) {
      this.showCaptcha = true;
    }
  }

  onCaptchaResolved(captchaResponse: string | null): void {
    this.captchaToken = captchaResponse;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      if (this.showCaptcha && !this.captchaToken) {
        this.error = 'Vui lòng xác thực CAPTCHA';
        return;
      }

      this.loading = true;
      this.error = '';

      const loginData = {
        ...this.loginForm.value,
        captchaToken: this.captchaToken
      };

      this.authService.login(loginData).subscribe(
        response => {
          // Reset failed attempts on success
          localStorage.removeItem('loginAttempts');
          
          if (response.requiresTwoFactor) {
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
          this.handleLoginError(error);
        }
      );
    }
  }

  private handleLoginError(error: any): void {
    this.error = error.error?.message || 'Đăng nhập thất bại';
    this.loading = false;
    
    // Increment failed attempts
    this.failedAttempts++;
    localStorage.setItem('loginAttempts', this.failedAttempts.toString());
    
    // Show captcha after 3 failed attempts
    if (this.failedAttempts >= 3) {
      this.showCaptcha = true;
    }
    
    // Reset captcha
    if (this.captchaRef) {
      this.captchaRef.reset();
      this.captchaToken = null;
    }
  }
}