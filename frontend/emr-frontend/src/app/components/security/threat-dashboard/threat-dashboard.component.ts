// threat-dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { ThreatIntelligenceService, ThreatIndicator, IpBlacklist } from '../../services/threat-intelligence.service';
import { interval } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-threat-dashboard',
  templateUrl: './threat-dashboard.component.html',
  styleUrls: ['./threat-dashboard.component.css']
})
export class ThreatDashboardComponent implements OnInit {
  threats: ThreatIndicator[] = [];
  blacklist: IpBlacklist[] = [];
  
  displayedColumnsThreats = ['type', 'value', 'severity', 'detectedAt'];
  displayedColumnsBlacklist = ['ipAddress', 'reason', 'blockedAt', 'expiresAt', 'actions'];

  constructor(private threatService: ThreatIntelligenceService) {}

  ngOnInit(): void {
    // Auto-refresh every 30 seconds
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.threatService.getActiveThreatIndicators())
    ).subscribe(
      data => {
        this.threats = data;
      },
      error => console.error('Error loading threats:', error)
    );

    this.loadBlacklist();
  }

  loadBlacklist(): void {
    this.threatService.getBlacklist().subscribe(
      data => {
        this.blacklist = data;
      },
      error => console.error('Error loading blacklist:', error)
    );
  }

  getSeverityColor(severity: number): string {
    if (severity >= 8) return 'warn';
    if (severity >= 5) return 'accent';
    return 'primary';
  }

  getSeverityLabel(severity: number): string {
    if (severity >= 8) return 'Cao';
    if (severity >= 5) return 'Trung bình';
    return 'Thấp';
  }

  unblockIp(item: IpBlacklist): void {
    if (confirm(`Bỏ chặn IP ${item.ipAddress}?`)) {
      this.threatService.unblockIpAddress(item.id).subscribe(
        () => this.loadBlacklist(),
        error => console.error('Error unblocking IP:', error)
      );
    }
  }
}