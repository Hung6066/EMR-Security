// appointment-form-dialog.component.ts
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AppointmentService } from '../../services/appointment.service';
import { DoctorService } from '../../services/doctor.service';
import { PatientService } from '../../services/patient.service';

@Component({
  selector: 'app-appointment-form-dialog',
  templateUrl: './appointment-form-dialog.component.html',
  styleUrls: ['./appointment-form-dialog.component.css']
})
export class AppointmentFormDialogComponent implements OnInit {
  appointmentForm: FormGroup;
  doctors: any[] = [];
  patients: any[] = [];
  timeSlots: string[] = [];
  isEditMode = false;
  
  statuses = ['Pending', 'Confirmed', 'Completed', 'Cancelled'];

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    public dialogRef: MatDialogRef<AppointmentFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.appointmentForm = this.fb.group({
      patientId: ['', Validators.required],
      doctorId: ['', Validators.required],
      appointmentDate: [data.date || new Date(), Validators.required],
      appointmentTime: ['', Validators.required],
      durationMinutes: [30, [Validators.required, Validators.min(15)]],
      reason: ['', Validators.required],
      notes: [''],
      status: ['Pending']
    });

    if (data.appointment) {
      this.isEditMode = true;
      this.appointmentForm.patchValue({
        ...data.appointment,
        appointmentDate: new Date(data.appointment.appointmentDate)
      });
    }
  }

  ngOnInit(): void {
    this.loadDoctors();
    this.loadPatients();
    this.generateTimeSlots();
  }

  loadDoctors(): void {
    this.doctorService.getAll().subscribe(
      data => {
        this.doctors = data;
      },
      error => console.error('Error loading doctors:', error)
    );
  }

  loadPatients(): void {
    this.patientService.getAll().subscribe(
      data => {
        this.patients = data;
      },
      error => console.error('Error loading patients:', error)
    );
  }

  generateTimeSlots(): void {
    this.timeSlots = [];
    for (let hour = 8; hour <= 17; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        const time = `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}:00`;
        this.timeSlots.push(time);
      }
    }
  }

  checkAvailability(): void {
    const { doctorId, appointmentDate, appointmentTime } = this.appointmentForm.value;
    
    if (doctorId && appointmentDate && appointmentTime) {
      this.appointmentService.checkAvailability(doctorId, appointmentDate, appointmentTime)
        .subscribe(
          result => {
            if (!result.isAvailable) {
              alert('Khung giờ này đã có lịch hẹn. Vui lòng chọn giờ khác.');
            }
          },
          error => console.error('Error checking availability:', error)
        );
    }
  }

  save(): void {
    if (this.appointmentForm.valid) {
      if (this.isEditMode) {
        const { status, notes } = this.appointmentForm.value;
        this.appointmentService.updateStatus(this.data.appointment.id, status, notes)
          .subscribe(
            () => {
              alert('Cập nhật thành công!');
              this.dialogRef.close(true);
            },
            error => {
              console.error('Error updating appointment:', error);
              alert('Cập nhật thất bại!');
            }
          );
      } else {
        this.appointmentService.create(this.appointmentForm.value).subscribe(
          () => {
            alert('Đặt lịch thành công!');
            this.dialogRef.close(true);
          },
          error => {
            console.error('Error creating appointment:', error);
            alert('Đặt lịch thất bại!');
          }
        );
      }
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }

  cancelAppointment(): void {
    const reason = prompt('Lý do hủy:');
    if (reason) {
      this.appointmentService.cancel(this.data.appointment.id, reason).subscribe(
        () => {
          alert('Hủy lịch thành công!');
          this.dialogRef.close(true);
        },
        error => {
          console.error('Error cancelling appointment:', error);
          alert('Hủy lịch thất bại!');
        }
      );
    }
  }
}