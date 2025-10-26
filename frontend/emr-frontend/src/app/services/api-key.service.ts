// api-key.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ApiKey {
  id: number;
  name: string;
  keyPrefix: string;
  createdAt: Date;
  expiresAt?: Date;
  lastUsedAt?: Date;
  isActive: boolean;
  isRevoked: boolean;
  ipWhitelist?: string;
  allowedScopes?: string;
  rateLimitPerMinute?: number;
}

export interface CreateApiKey {
  name: string;
  expiresAt?: Date;
  ipWhitelist?: string;
  scopes?: string[];
  rateLimitPerMinute?: number;
}

export interface ApiKeyResponse {
  id: number;
  name: string;
  apiKey: string;
  keyPrefix: string;
  createdAt: Date;
  expiresAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ApiKeyService {
  private apiUrl = 'http://localhost:5000/api/apikeys';

  constructor(private http: HttpClient) {}

  getApiKeys(): Observable<ApiKey[]> {
    return this.http.get<ApiKey[]>(this.apiUrl);
  }

  createApiKey(data: CreateApiKey): Observable<ApiKeyResponse> {
    return this.http.post<ApiKeyResponse>(this.apiUrl, data);
  }

  revokeApiKey(keyId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${keyId}`);
  }
}