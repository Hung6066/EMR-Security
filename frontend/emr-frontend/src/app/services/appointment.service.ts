// appointment.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Appointment {
  id: number;
  patientId: number;
  patientName: string;
  doctorId: number;
  doctorName: string;
  appointmentDate: Date;
  appointmentTime: string;
  durationMinutes: number;
  status: string;
  reason: string;
  notes?: string;
}

export interface CreateAppointment {
  patientId: number;
  doctorId: number;
  appointmentDate: Date;
  appointmentTime: string;
  durationMinutes: number;
  reason: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = 'http://localhost:5000/api/appointments';

  constructor(private http: HttpClient) {}

  create(appointment: CreateAppointment): Observable<Appointment> {
    return this.http.post<Appointment>(this.apiUrl, appointment);
  }

  getById(id: number): Observable<Appointment> {
    return this.http.get<Appointment>(`${this.apiUrl}/${id}`);
  }

  getByPatientId(patientId: number): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.apiUrl}/patient/${patientId}`);
  }

  getByDoctorId(doctorId: number, date: Date): Observable<Appointment[]> {
    const dateStr = date.toISOString().split('T')[0];
    return this.http.get<Appointment[]>(`${this.apiUrl}/doctor/${doctorId}?date=${dateStr}`);
  }

  getByDateRange(startDate: Date, endDate: Date): Observable<Appointment[]> {
    const start = startDate.toISOString().split('T')[0];
    const end = endDate.toISOString().split('T')[0];
    return this.http.get<Appointment[]>(`${this.apiUrl}/range?startDate=${start}&endDate=${end}`);
  }

  updateStatus(id: number, status: string, notes?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, { status, notes });
  }

  cancel(id: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, JSON.stringify(reason), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  checkAvailability(doctorId: number, date: Date, time: string): Observable<{ isAvailable: boolean }> {
    const dateStr = date.toISOString().split('T')[0];
    return this.http.get<{ isAvailable: boolean }>(
      `${this.apiUrl}/check-availability?doctorId=${doctorId}&date=${dateStr}&time=${time}`
    );
  }
}