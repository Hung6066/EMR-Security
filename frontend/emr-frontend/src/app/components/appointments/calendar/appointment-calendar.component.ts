// appointment-calendar.component.ts
import { Component, OnInit } from '@angular/core';
import { AppointmentService, Appointment } from '../../services/appointment.service';
import { MatDialog } from '@angular/material/dialog';
import { AppointmentFormDialogComponent } from './appointment-form-dialog.component';

@Component({
  selector: 'app-appointment-calendar',
  templateUrl: './appointment-calendar.component.html',
  styleUrls: ['./appointment-calendar.component.css']
})
export class AppointmentCalendarComponent implements OnInit {
  appointments: Appointment[] = [];
  selectedDate = new Date();
  calendarDays: Date[] = [];
  
  statusColors: { [key: string]: string } = {
    'Pending': '#FFA726',
    'Confirmed': '#66BB6A',
    'Cancelled': '#EF5350',
    'Completed': '#42A5F5'
  };

  constructor(
    private appointmentService: AppointmentService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.generateCalendar();
    this.loadAppointments();
  }

  generateCalendar(): void {
    const year = this.selectedDate.getFullYear();
    const month = this.selectedDate.getMonth();
    
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - startDate.getDay());
    
    const endDate = new Date(lastDay);
    endDate.setDate(endDate.getDate() + (6 - endDate.getDay()));
    
    this.calendarDays = [];
    const currentDate = new Date(startDate);
    
    while (currentDate <= endDate) {
      this.calendarDays.push(new Date(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }
  }

  loadAppointments(): void {
    const startDate = this.calendarDays[0];
    const endDate = this.calendarDays[this.calendarDays.length - 1];
    
    this.appointmentService.getByDateRange(startDate, endDate).subscribe(
      data => {
        this.appointments = data;
      },
      error => console.error('Error loading appointments:', error)
    );
  }

  getAppointmentsForDay(date: Date): Appointment[] {
    return this.appointments.filter(apt => {
      const aptDate = new Date(apt.appointmentDate);
      return aptDate.toDateString() === date.toDateString();
    });
  }

  previousMonth(): void {
    this.selectedDate.setMonth(this.selectedDate.getMonth() - 1);
    this.generateCalendar();
    this.loadAppointments();
  }

  nextMonth(): void {
    this.selectedDate.setMonth(this.selectedDate.getMonth() + 1);
    this.generateCalendar();
    this.loadAppointments();
  }

  today(): void {
    this.selectedDate = new Date();
    this.generateCalendar();
    this.loadAppointments();
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  isCurrentMonth(date: Date): boolean {
    return date.getMonth() === this.selectedDate.getMonth();
  }

  openAppointmentDialog(date?: Date): void {
    const dialogRef = this.dialog.open(AppointmentFormDialogComponent, {
      width: '600px',
      data: { date: date || new Date() }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadAppointments();
      }
    });
  }

  viewAppointment(appointment: Appointment): void {
    const dialogRef = this.dialog.open(AppointmentFormDialogComponent, {
      width: '600px',
      data: { appointment }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadAppointments();
      }
    });
  }

  getMonthYear(): string {
    return this.selectedDate.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' });
  }
}