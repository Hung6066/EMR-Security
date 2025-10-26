// webauthn.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { switchMap } from 'rxjs/operators';

export interface WebAuthnCredential {
  id: number;
  deviceName: string;
  createdAt: Date;
  lastUsedAt?: Date;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class WebAuthnService {
  private apiUrl = 'http://localhost:5000/api/webauthn';

  constructor(private http: HttpClient) {}

  // Check browser support
  isWebAuthnSupported(): boolean {
    return window.PublicKeyCredential !== undefined &&
           typeof window.PublicKeyCredential === 'function';
  }

  // Register new credential
  registerCredential(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register/options`, {}).pipe(
      switchMap(options => {
        // Decode base64 values
        const publicKeyOptions = this.decodePublicKeyOptions(options);
        
        // Call WebAuthn API
        return from(navigator.credentials.create({ publicKey: publicKeyOptions }));
      }),
      switchMap(credential => {
        const attestationResponse = this.encodeAttestationResponse(credential);
        return this.http.post(`${this.apiUrl}/register`, attestationResponse);
      })
    );
  }

  // Login with credential
  loginWithBiometric(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login/options`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      switchMap(options => {
        const publicKeyOptions = this.decodeAssertionOptions(options);
        return from(navigator.credentials.get({ publicKey: publicKeyOptions }));
      }),
      switchMap(credential => {
        const assertionResponse = this.encodeAssertionResponse(credential);
        return this.http.post(`${this.apiUrl}/login?email=${email}`, assertionResponse);
      })
    );
  }

  // Get user's credentials
  getCredentials(): Observable<WebAuthnCredential[]> {
    return this.http.get<WebAuthnCredential[]>(`${this.apiUrl}/credentials`);
  }

  // Revoke credential
  revokeCredential(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/credentials/${id}`);
  }

  // Helper methods for encoding/decoding
  private decodePublicKeyOptions(options: any): PublicKeyCredentialCreationOptions {
    return {
      ...options,
      challenge: this.base64ToArrayBuffer(options.challenge),
      user: {
        ...options.user,
        id: this.base64ToArrayBuffer(options.user.id)
      },
      excludeCredentials: options.excludeCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64ToArrayBuffer(cred.id)
      }))
    };
  }

  private decodeAssertionOptions(options: any): PublicKeyCredentialRequestOptions {
    return {
      ...options,
      challenge: this.base64ToArrayBuffer(options.challenge),
      allowCredentials: options.allowCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64ToArrayBuffer(cred.id)
      }))
    };
  }

  private encodeAttestationResponse(credential: any): any {
    const response = credential.response as AuthenticatorAttestationResponse;
    return {
      id: credential.id,
      rawId: this.arrayBufferToBase64(credential.rawId),
      type: credential.type,
      response: {
        attestationObject: this.arrayBufferToBase64(response.attestationObject),
        clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON)
      }
    };
  }

  private encodeAssertionResponse(credential: any): any {
    const response = credential.response as AuthenticatorAssertionResponse;
    return {
      id: credential.id,
      rawId: this.arrayBufferToBase64(credential.rawId),
      type: credential.type,
      response: {
        authenticatorData: this.arrayBufferToBase64(response.authenticatorData),
        clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON),
        signature: this.arrayBufferToBase64(response.signature),
        userHandle: response.userHandle ? this.arrayBufferToBase64(response.userHandle) : null
      }
    };
  }

  private base64ToArrayBuffer(base64: string): ArrayBuffer {
    const binaryString = window.atob(base64);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }
}