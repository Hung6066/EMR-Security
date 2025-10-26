// medical-record-form.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MedicalRecordService } from '../../services/medical-record.service';

@Component({
  selector: 'app-medical-record-form',
  templateUrl: './medical-record-form.component.html',
  styleUrls: ['./medical-record-form.component.css']
})
export class MedicalRecordFormComponent implements OnInit {
  recordForm: FormGroup;
  patientId: number;

  constructor(
    private fb: FormBuilder,
    private recordService: MedicalRecordService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.recordForm = this.fb.group({
      patientId: ['', Validators.required],
      doctorId: ['', Validators.required],
      visitDate: [new Date(), Validators.required],
      chiefComplaint: ['', Validators.required],
      presentIllness: [''],
      physicalExamination: [''],
      diagnosis: [''],
      treatment: [''],
      notes: [''],
      vitalSigns: this.fb.group({
        temperature: [''],
        bloodPressure: [''],
        heartRate: [''],
        respiratoryRate: [''],
        weight: [''],
        height: ['']
      })
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['patientId']) {
        this.patientId = +params['patientId'];
        this.recordForm.patchValue({ patientId: this.patientId });
      }
    });
  }

  onSubmit(): void {
    if (this.recordForm.valid) {
      this.recordService.create(this.recordForm.value).subscribe(
        () => {
          alert('Thêm bệnh án thành công!');
          this.router.navigate(['/patients', this.patientId]);
        },
        error => console.error('Error creating record', error)
      );
    }
  }

  cancel(): void {
    this.router.navigate(['/patients', this.patientId]);
  }

//   printRecord(): void {
//   this.medicalRecordService.downloadPdf(this.recordId).subscribe(
//     blob => {
//       const url = window.URL.createObjectURL(blob);
//       const link = document.createElement('a');
//       link.href = url;
//       link.download = `BenhAn_${this.recordId}_${new Date().getTime()}.pdf`;
//       link.click();
//       window.URL.revokeObjectURL(url);
//     },
//     error => {
//       console.error('Error downloading PDF:', error);
//       alert('Không thể tải file PDF');
//     }
//   );
// }

// <!-- Add to medical-record-detail.component.html -->
// <button mat-raised-button color="primary" (click)="printRecord()">
//   <mat-icon>print</mat-icon>
//   In Bệnh án
// </button>
}