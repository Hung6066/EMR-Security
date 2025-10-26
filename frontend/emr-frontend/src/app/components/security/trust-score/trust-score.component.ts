// trust-score.component.ts
import { Component, OnInit } from '@angular/core';
import { ZeroTrustService, TrustScore } from '../../services/zero-trust.service';
import { interval } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-trust-score',
  templateUrl: './trust-score.component.html',
  styleUrls: ['./trust-score.component.css']
})
export class TrustScoreComponent implements OnInit {
  trustScore: TrustScore | null = null;
  loading = true;

  constructor(private zeroTrustService: ZeroTrustService) {}

  ngOnInit(): void {
    // Auto-refresh every 60 seconds
    interval(60000).pipe(
      startWith(0),
      switchMap(() => this.zeroTrustService.getTrustScore())
    ).subscribe(
      data => {
        this.trustScore = data;
        this.loading = false;
      },
      error => {
        console.error('Error loading trust score:', error);
        this.loading = false;
      }
    );
  }

  getTrustLevel(): string {
    if (!this.trustScore) return 'Unknown';
    const score = this.trustScore.overallScore;
    if (score >= 80) return 'Excellent';
    if (score >= 60) return 'Good';
    if (score >= 40) return 'Fair';
    return 'Poor';
  }

  getTrustColor(): string {
    if (!this.trustScore) return 'warn';
    const score = this.trustScore.overallScore;
    if (score >= 80) return 'primary';
    if (score >= 60) return 'accent';
    return 'warn';
  }

  getScoreColor(score: number): string {
    if (score >= 80) return '#4caf50';
    if (score >= 60) return '#2196f3';
    if (score >= 40) return '#ff9800';
    return '#f44336';
  }
}