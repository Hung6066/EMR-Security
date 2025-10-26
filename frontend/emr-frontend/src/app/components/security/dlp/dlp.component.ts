import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DlpService, DlpRule, DlpIncident } from '../../../services/dlp.service'; // Tạo service này

@Component({
  selector: 'app-dlp',
  templateUrl: './dlp.component.html',
  styleUrls: ['./dlp.component.css']
})
export class DlpComponent implements OnInit {
  // Rules
  rules: DlpRule[] = [];
  ruleDisplayedColumns = ['name', 'dataType', 'severity', 'action', 'isActive', 'actions'];
  editingRule: DlpRule | null = null;
  ruleForm: FormGroup;
  
  // Incidents
  incidents: DlpIncident[] = [];
  incidentDisplayedColumns = ['detectedAt', 'userName', 'channel', 'ruleName', 'actionTaken', 'context'];
  incidentFilterForm: FormGroup;

  constructor(
    private dlpService: DlpService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    // Rule Form
    this.ruleForm = this.fb.group({
      id: [0],
      name: [''],
      description: [''],
      pattern: [''],
      dataType: ['PII'],
      severity: [3],
      action: ['Alert'],
      isActive: [true]
    });

    // Incident Filter Form
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    this.incidentFilterForm = this.fb.group({
      range: this.fb.group({
        from: [yesterday],
        to: [new Date()]
      })
    });
  }

  ngOnInit(): void {
    this.loadRules();
    this.loadIncidents();
  }

  // --- Rules Logic ---
  loadRules(): void {
    this.dlpService.getRules().subscribe(data => this.rules = data);
  }

  editRule(rule: DlpRule): void {
    this.editingRule = { ...rule };
    this.ruleForm.patchValue(this.editingRule);
  }

  cancelEdit(): void {
    this.editingRule = null;
    this.ruleForm.reset({
      id: 0, dataType: 'PII', severity: 3, action: 'Alert', isActive: true
    });
  }

  saveRule(): void {
    if (this.ruleForm.valid) {
      this.dlpService.upsertRule(this.ruleForm.value).subscribe(() => {
        this.snackBar.open('Rule saved successfully!', 'OK', { duration: 3000 });
        this.loadRules();
        this.cancelEdit();
      });
    }
  }

  // --- Incidents Logic ---
  loadIncidents(): void {
    const { from, to } = this.incidentFilterForm.get('range')?.value;
    if (from && to) {
      this.dlpService.getIncidents(from.toISOString(), to.toISOString()).subscribe(data => this.incidents = data);
    }
  }

  getSeverityColor(severity: number): string {
    if (severity >= 4) return 'warn';
    if (severity >= 3) return 'accent';
    return 'primary';
  }
}