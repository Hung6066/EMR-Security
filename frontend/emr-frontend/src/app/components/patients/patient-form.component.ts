// patient-form.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PatientService } from '../../services/patient.service';

@Component({
  selector: 'app-patient-form',
  templateUrl: './patient-form.component.html',
  styleUrls: ['./patient-form.component.css']
})
export class PatientFormComponent implements OnInit {
  patientForm: FormGroup;
  isEditMode = false;
  patientId: number;

  genders = ['Nam', 'Nữ', 'Khác'];
  bloodTypes = ['A', 'B', 'AB', 'O', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.patientForm = this.fb.group({
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      identityCard: [''],
      phoneNumber: [''],
      email: ['', Validators.email],
      address: [''],
      bloodType: [''],
      allergies: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id'] && params['id'] !== 'new') {
        this.isEditMode = true;
        this.patientId = +params['id'];
        this.loadPatient();
      }
    });
  }

  loadPatient(): void {
    this.patientService.getById(this.patientId).subscribe(
      data => {
        this.patientForm.patchValue({
          ...data,
          dateOfBirth: new Date(data.dateOfBirth)
        });
      },
      error => console.error('Error loading patient', error)
    );
  }

  onSubmit(): void {
    if (this.patientForm.valid) {
      const patientData = this.patientForm.value;
      
      if (this.isEditMode) {
        this.patientService.update(this.patientId, patientData).subscribe(
          () => {
            alert('Cập nhật thành công!');
            this.router.navigate(['/patients']);
          },
          error => console.error('Error updating patient', error)
        );
      } else {
        this.patientService.create(patientData).subscribe(
          () => {
            alert('Thêm mới thành công!');
            this.router.navigate(['/patients']);
          },
          error => console.error('Error creating patient', error)
        );
      }
    }
  }

  cancel(): void {
    this.router.navigate(['/patients']);
  }
}