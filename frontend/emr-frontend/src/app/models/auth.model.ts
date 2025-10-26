// auth.model.ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber?: string;
  role?: string;
}

export interface AuthResponse {
  userId: number;
  fullName: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  roles: string[];
}

export interface CurrentUser {
  userId: number;
  userName: string;
  email: string;
  roles: string[];
}