// report.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStats {
  totalPatients: number;
  totalAppointmentsToday: number;
  pendingAppointments: number;
  completedAppointmentsThisMonth: number;
  newPatientsThisMonth: number;
}

export interface PatientStatistics {
  patientsByGender: { [key: string]: number };
  patientsByAgeGroup: { [key: string]: number };
  patientsByBloodType: { [key: string]: number };
}

export interface AppointmentStatistics {
  appointmentsByStatus: { [key: string]: number };
  appointmentsByDoctor: { [key: string]: number };
  appointmentsByDay: { date: Date; count: number }[];
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = 'http://localhost:5000/api/reports';

  constructor(private http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  getPatientStatistics(): Observable<PatientStatistics> {
    return this.http.get<PatientStatistics>(`${this.apiUrl}/patients`);
  }

  getAppointmentStatistics(startDate: Date, endDate: Date): Observable<AppointmentStatistics> {
    const start = startDate.toISOString().split('T')[0];
    const end = endDate.toISOString().split('T')[0];
    return this.http.get<AppointmentStatistics>(
      `${this.apiUrl}/appointments?startDate=${start}&endDate=${end}`
    );
  }
}