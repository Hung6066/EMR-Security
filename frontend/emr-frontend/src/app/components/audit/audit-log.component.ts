// audit-log.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AuditService, AuditLog, AuditLogFilter } from '../../services/audit.service';
import { MatDialog } from '@angular/material/dialog';
import { AuditDetailDialogComponent } from './audit-detail-dialog.component';

@Component({
  selector: 'app-audit-log',
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.css']
})
export class AuditLogComponent implements OnInit {
  filterForm: FormGroup;
  auditLogs: AuditLog[] = [];
  loading = false;
  
  displayedColumns = ['timestamp', 'userName', 'action', 'entityType', 'entityId', 'ipAddress', 'status', 'actions'];
  
  actions = ['CREATE', 'READ', 'UPDATE', 'DELETE', 'EXPORT', 'LOGIN', 'LOGOUT'];
  entityTypes = ['Patient', 'MedicalRecord', 'Prescription', 'User', 'Appointment'];
  
  pageSize = 50;
  pageNumber = 1;
  totalRecords = 0;

  constructor(
    private fb: FormBuilder,
    private auditService: AuditService,
    private dialog: MatDialog
  ) {
    this.filterForm = this.fb.group({
      action: [''],
      entityType: [''],
      startDate: [null],
      endDate: [null]
    });
  }

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading = true;
    
    const filter: AuditLogFilter = {
      ...this.filterForm.value,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.auditService.getAuditLogs(filter).subscribe(
      data => {
        this.auditLogs = data;
        this.loading = false;
      },
      error => {
        console.error('Error loading audit logs:', error);
        this.loading = false;
      }
    );
  }

  applyFilter(): void {
    this.pageNumber = 1;
    this.loadAuditLogs();
  }

  clearFilter(): void {
    this.filterForm.reset();
    this.pageNumber = 1;
    this.loadAuditLogs();
  }

  viewDetails(log: AuditLog): void {
    this.dialog.open(AuditDetailDialogComponent, {
      width: '800px',
      data: log
    });
  }

  exportLogs(): void {
    const filter: AuditLogFilter = this.filterForm.value;
    
    this.auditService.exportAuditLogs(filter).subscribe(
      blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `audit-logs-${new Date().getTime()}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error => console.error('Error exporting logs:', error)
    );
  }

  getActionColor(action: string): string {
    const colors: { [key: string]: string } = {
      'CREATE': 'primary',
      'READ': 'accent',
      'UPDATE': 'warn',
      'DELETE': 'warn',
      'EXPORT': 'warn',
      'LOGIN': 'primary',
      'LOGOUT': 'accent'
    };
    return colors[action] || '';
  }

  getActionIcon(action: string): string {
    const icons: { [key: string]: string } = {
      'CREATE': 'add_circle',
      'READ': 'visibility',
      'UPDATE': 'edit',
      'DELETE': 'delete',
      'EXPORT': 'file_download',
      'LOGIN': 'login',
      'LOGOUT': 'logout'
    };
    return icons[action] || 'info';
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadAuditLogs();
    }
  }

  nextPage(): void {
    this.pageNumber++;
    this.loadAuditLogs();
  }
}