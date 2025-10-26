// security-incidents.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { SecurityIncidentService, SecurityIncident } from '../../services/security-incident.service';
import { IncidentDetailDialogComponent } from './incident-detail-dialog.component';
import { CreateIncidentDialogComponent } from './create-incident-dialog.component';

@Component({
  selector: 'app-security-incidents',
  templateUrl: './security-incidents.component.html',
  styleUrls: ['./security-incidents.component.css']
})
export class SecurityIncidentsComponent implements OnInit {
  incidents: SecurityIncident[] = [];
  displayedColumns = ['severity', 'title', 'category', 'status', 'detectedAt', 'assignedTo', 'actions'];
  
  severityFilter = 'all';
  statusFilter = 'all';

  constructor(
    private incidentService: SecurityIncidentService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadIncidents();
  }

  loadIncidents(): void {
    this.incidentService.getActiveIncidents().subscribe(
      data => {
        this.incidents = data;
      },
      error => console.error('Error loading incidents:', error)
    );
  }

  getFilteredIncidents(): SecurityIncident[] {
    return this.incidents.filter(incident => {
      const severityMatch = this.severityFilter === 'all' || incident.severity === this.severityFilter;
      const statusMatch = this.statusFilter === 'all' || incident.status === this.statusFilter;
      return severityMatch && statusMatch;
    });
  }

  createIncident(): void {
    const dialogRef = this.dialog.open(CreateIncidentDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadIncidents();
      }
    });
  }

  viewIncident(incident: SecurityIncident): void {
    const dialogRef = this.dialog.open(IncidentDetailDialogComponent, {
      width: '900px',
      maxHeight: '90vh',
      data: incident
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadIncidents();
      }
    });
  }

  getSeverityColor(severity: string): string {
    const colors: { [key: string]: string } = {
      'Critical': 'critical',
      'High': 'high',
      'Medium': 'medium',
      'Low': 'low'
    };
    return colors[severity] || '';
  }

  getSeverityIcon(severity: string): string {
    const icons: { [key: string]: string } = {
      'Critical': 'report',
      'High': 'warning',
      'Medium': 'info',
      'Low': 'flag'
    };
    return icons[severity] || 'flag';
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'New': 'warn',
      'Investigating': 'accent',
      'Contained': 'primary',
      'Resolved': 'primary',
      'Closed': ''
    };
    return colors[status] || '';
  }
}