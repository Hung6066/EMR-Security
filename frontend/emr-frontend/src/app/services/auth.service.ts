// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const token = this.getToken();
    if (token) {
      this.loadCurrentUser();
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          this.setToken(response.accessToken);
          this.setRefreshToken(response.refreshToken);
          this.currentUserSubject.next({
            userId: response.userId,
            userName: response.fullName,
            email: response.email,
            roles: response.roles
          });
        })
      );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data)
      .pipe(
        tap(response => {
          this.setToken(response.accessToken);
          this.setRefreshToken(response.refreshToken);
        })
      );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke-token`, { refreshToken }).subscribe();
    }
    this.clearTokens();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(
        tap(response => {
          this.setToken(response.accessToken);
          this.setRefreshToken(response.refreshToken);
        })
      );
  }

  loadCurrentUser(): void {
    this.http.get<CurrentUser>(`${this.apiUrl}/me`).subscribe(
      user => this.currentUserSubject.next(user),
      error => this.logout()
    );
  }

  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  private setToken(token: string): void {
    localStorage.setItem('accessToken', token);
  }

  private setRefreshToken(token: string): void {
    localStorage.setItem('refreshToken', token);
  }

  private clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles?.includes(role) || false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    return roles.some(role => user?.roles?.includes(role)) || false;
  }

  loginWith2FA(credentials: any): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.apiUrl}/login-2fa`, credentials)
    .pipe(
      tap(response => {
        this.setToken(response.accessToken);
        this.setRefreshToken(response.refreshToken);
        this.currentUserSubject.next({
          userId: response.userId,
          userName: response.fullName,
          email: response.email,
          roles: response.roles
        });
      })
    );
}

changePassword(data: any): Observable<void> {
  return this.http.post<void>(`${this.apiUrl}/change-password`, data);
}

forgotPassword(email: string): Observable<void> {
  return this.http.post<void>(`${this.apiUrl}/forgot-password`, JSON.stringify(email), {
    headers: { 'Content-Type': 'application/json' }
  });
}

resetPassword(data: { email: string; token: string; newPassword: string }): Observable<void> {
  return this.http.post<void>(`${this.apiUrl}/reset-password`, data);
}

verifyEmail(email: string, token: string): Observable<void> {
  return this.http.post<void>(`${this.apiUrl}/verify-email`, { email, token });
}
}