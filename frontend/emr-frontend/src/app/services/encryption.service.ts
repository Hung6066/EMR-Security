// encryption.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EncryptionKey {
  id: number;
  keyName: string;
  keyType: string;
  purpose: string;
  keySize: number;
  createdAt: Date;
  expiresAt: Date;
  isActive: boolean;
  isRotated: boolean;
  rotatedAt?: Date;
}

export interface VaultEntry {
  id: number;
  keyName: string;
  description: string;
  createdAt: Date;
  updatedAt?: Date;
  expiresAt?: Date;
}

export interface EncryptionMetrics {
  totalKeys: number;
  activeKeys: number;
  expiringSoonKeys: number;
  rotatedKeys: number;
  encryptionOperations: number;
  decryptionOperations: number;
}

@Injectable({
  providedIn: 'root'
})
export class EncryptionService {
  private apiUrl = 'http://localhost:5000/api/encryption';

  constructor(private http: HttpClient) {}

  getKeys(): Observable<EncryptionKey[]> {
    return this.http.get<EncryptionKey[]>(`${this.apiUrl}/keys`);
  }

  createKey(keyData: any): Observable<EncryptionKey> {
    return this.http.post<EncryptionKey>(`${this.apiUrl}/keys`, keyData);
  }

  rotateKey(keyId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/keys/${keyId}/rotate`, {});
  }

  getVaultEntries(): Observable<VaultEntry[]> {
    return this.http.get<VaultEntry[]>(`${this.apiUrl}/vault`);
  }

  storeInVault(data: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/vault`, data);
  }

  getFromVault(keyName: string): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/vault/${keyName}`);
  }

  deleteFromVault(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/vault/${id}`);
  }

  getMetrics(): Observable<EncryptionMetrics> {
    return this.http.get<EncryptionMetrics>(`${this.apiUrl}/metrics`);
  }

  encryptData(data: string, purpose: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/encrypt`, { data, purpose });
  }

  decryptData(encryptedData: any): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/decrypt`, encryptedData);
  }
}