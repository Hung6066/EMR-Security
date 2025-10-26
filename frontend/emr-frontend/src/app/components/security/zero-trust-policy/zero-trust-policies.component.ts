// zero-trust-policies.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ZeroTrustService, ZeroTrustPolicy } from '../../services/zero-trust.service';
import { PolicyFormDialogComponent } from './policy-form-dialog.component';

@Component({
  selector: 'app-zero-trust-policies',
  templateUrl: './zero-trust-policies.component.html',
  styleUrls: ['./zero-trust-policies.component.css']
})
export class ZeroTrustPoliciesComponent implements OnInit {
  policies: ZeroTrustPolicy[] = [];
  displayedColumns = ['name', 'resourceType', 'minTrustScore', 'requirements', 'status', 'actions'];

  constructor(
    private zeroTrustService: ZeroTrustService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.zeroTrustService.getPolicies().subscribe(
      data => {
        this.policies = data;
      },
      error => console.error('Error loading policies:', error)
    );
  }

  createPolicy(): void {
    const dialogRef = this.dialog.open(PolicyFormDialogComponent, {
      width: '700px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadPolicies();
      }
    });
  }

  editPolicy(policy: ZeroTrustPolicy): void {
    const dialogRef = this.dialog.open(PolicyFormDialogComponent, {
      width: '700px',
      data: policy
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadPolicies();
      }
    });
  }

  togglePolicy(policy: ZeroTrustPolicy): void {
    const newStatus = !policy.isActive;
    
    this.zeroTrustService.updatePolicy(policy.id, { isActive: newStatus }).subscribe(
      () => {
        policy.isActive = newStatus;
        this.snackBar.open(
          `Policy ${newStatus ? 'activated' : 'deactivated'}`,
          'Close',
          { duration: 3000 }
        );
      },
      error => {
        this.snackBar.open('Error updating policy', 'Close', { duration: 3000 });
      }
    );
  }

  deletePolicy(policy: ZeroTrustPolicy): void {
    if (confirm(`Are you sure you want to delete policy "${policy.name}"?`)) {
      this.zeroTrustService.deletePolicy(policy.id).subscribe(
        () => {
          this.snackBar.open('Policy deleted', 'Close', { duration: 3000 });
          this.loadPolicies();
        },
        error => {
          this.snackBar.open('Error deleting policy', 'Close', { duration: 3000 });
        }
      );
    }
  }

  getRequirements(policy: ZeroTrustPolicy): string[] {
    const requirements: string[] = [];
    if (policy.requiresMFA) requirements.push('MFA');
    if (policy.requiresDeviceCompliance) requirements.push('Device Compliance');
    if (policy.requiresNetworkCompliance) requirements.push('Network Compliance');
    return requirements;
  }
}