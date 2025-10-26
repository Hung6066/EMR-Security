// patient-list.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient.model';

@Component({
  selector: 'app-patient-list',
  templateUrl: './patient-list.component.html',
  styleUrls: ['./patient-list.component.css']
})
export class PatientListComponent implements OnInit {
  patients: Patient[] = [];
  displayedColumns: string[] = ['id', 'fullName', 'dateOfBirth', 'gender', 'phoneNumber', 'actions'];
  searchTerm = '';

  constructor(
    private patientService: PatientService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.patientService.getAll().subscribe(
      data => this.patients = data,
      error => console.error('Error loading patients', error)
    );
  }

  search(): void {
    if (this.searchTerm.trim()) {
      this.patientService.search(this.searchTerm).subscribe(
        data => this.patients = data,
        error => console.error('Error searching patients', error)
      );
    } else {
      this.loadPatients();
    }
  }

  viewDetails(id: number): void {
    this.router.navigate(['/patients', id]);
  }

  editPatient(id: number): void {
    this.router.navigate(['/patients', id, 'edit']);
  }

  deletePatient(id: number): void {
    if (confirm('Bạn có chắc muốn xóa bệnh nhân này?')) {
      this.patientService.delete(id).subscribe(
        () => this.loadPatients(),
        error => console.error('Error deleting patient', error)
      );
    }
  }

  createNew(): void {
    this.router.navigate(['/patients/new']);
  }
}