// privacy.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AnonymizationConfig {
  dataType: string;
  method: string; // Anonymization, Pseudonymization, Tokenization
  fields: string[];
}

export interface SyntheticDataRequest {
  sourceDataType: string;
  recordCount: number;
  preservePatterns: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PrivacyService {
  private apiUrl = 'http://localhost:5000/api/privacy';

  constructor(private http: HttpClient) {}

  anonymizeData(data: any, config: AnonymizationConfig): Observable<any> {
    return this.http.post(`${this.apiUrl}/anonymize`, { data, config });
  }

  pseudonymizeData(data: any, salt: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/pseudonymize`, { data, salt });
  }

  generateSyntheticData(request: SyntheticDataRequest): Observable<any[]> {
    return this.http.post<any[]>(`${this.apiUrl}/synthetic`, request);
  }

  tokenizeData(data: string, tokenType: string): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/tokenize`, { data, tokenType });
  }

  detokenizeData(token: string): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/detokenize`, { token });
  }

  applyDifferentialPrivacy(query: string, epsilon: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/differential-privacy`, { query, epsilon });
  }
}