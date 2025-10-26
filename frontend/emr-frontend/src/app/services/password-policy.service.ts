// src/app/services/password-policy.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PasswordPolicy {
  id: number;
  minLength: number;
  requireLowercase: boolean;
  requireUppercase: boolean;
  requireDigit: boolean;
  requireNonAlphanumeric: boolean;
  requiredUniqueChars: number;
  expireDays: number;
  passwordHistory: number;
  checkCommonPasswords: boolean;
  checkPwnedPasswords: boolean;
  maxFailedAccessAttempts: number;
  lockoutMinutes: number;
}

@Injectable({
  providedIn: 'root'
})
export class PasswordPolicyService {
  private apiUrl = 'http://localhost:5000/api/security/password-policy';

  constructor(private http: HttpClient) {}

  getPolicy(): Observable<PasswordPolicy> {
    return this.http.get<PasswordPolicy>(this.apiUrl);
  }

  updatePolicy(policy: PasswordPolicy): Observable<PasswordPolicy> {
    return this.http.put<PasswordPolicy>(this.apiUrl, policy);
  }
}