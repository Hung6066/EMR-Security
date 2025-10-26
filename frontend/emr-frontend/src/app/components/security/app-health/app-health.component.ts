import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface AttestationResult { isValid: boolean; mismatchedFiles: string[]; }
interface LibraryVulnerability { packageName: string; version: string; cveId: string; severity: string; }

@Component({
  selector: 'app-app-health',
  templateUrl: './app-health.component.html',
  styleUrls: ['./app-health.component.css']
})
export class AppHealthComponent implements OnInit {
  attestationResult: AttestationResult | null = null;
  dependenciesResult: LibraryVulnerability[] = [];
  loadingAttestation = true;
  loadingDeps = true;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.verifyCode();
    this.verifyDependencies();
  }

  verifyCode(): void {
    this.loadingAttestation = true;
    this.http.get<AttestationResult>('/api/attestation/verify-self').subscribe(data => {
      this.attestationResult = data;
      this.loadingAttestation = false;
    });
  }

  verifyDependencies(): void {
    this.loadingDeps = true;
    this.http.get<LibraryVulnerability[]>('/api/attestation/verify-deps').subscribe(data => {
      this.dependenciesResult = data;
      this.loadingDeps = false;
    });
  }
}