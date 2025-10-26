// audit-detail-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AuditLog } from '../../services/audit.service';

@Component({
  selector: 'app-audit-detail-dialog',
  templateUrl: './audit-detail-dialog.component.html',
  styleUrls: ['./audit-detail-dialog.component.css']
})
export class AuditDetailDialogComponent {
  oldValuesJson: string;
  newValuesJson: string;

  constructor(
    public dialogRef: MatDialogRef<AuditDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public log: AuditLog
  ) {
    this.oldValuesJson = log.oldValues 
      ? JSON.stringify(JSON.parse(log.oldValues), null, 2)
      : 'N/A';
    
    this.newValuesJson = log.newValues
      ? JSON.stringify(JSON.parse(log.newValues), null, 2)
      : 'N/A';
  }

  close(): void {
    this.dialogRef.close();
  }

  getDifferences(): any[] {
    if (!this.log.oldValues || !this.log.newValues) return [];

    const oldObj = JSON.parse(this.log.oldValues);
    const newObj = JSON.parse(this.log.newValues);
    const differences: any[] = [];

    for (const key in newObj) {
      if (oldObj[key] !== newObj[key]) {
        differences.push({
          field: key,
          oldValue: oldObj[key],
          newValue: newObj[key]
        });
      }
    }

    return differences;
  }
}