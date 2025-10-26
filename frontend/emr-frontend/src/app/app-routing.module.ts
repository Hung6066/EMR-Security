// app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

import { LoginComponent } from './components/auth/login.component';
import { RegisterComponent } from './components/auth/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { PatientListComponent } from './components/patients/patient-list.component';
import { PatientFormComponent } from './components/patients/patient-form.component';
import { MedicalRecordFormComponent } from './components/medical-records/medical-record-form.component';
import { AppointmentCalendarComponent } from './components/appointments/appointment-calendar.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'login-2fa', component: Login2FAComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'verify-email', component: VerifyEmailComponent },
    { path: 'patients', component: PatientListComponent },
      { path: 'appointments', component: AppointmentCalendarComponent },
      { path: 'reports', component: AdvancedReportsComponent, canActivate: [RoleGuard],
  { 
    path: '', 
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'enable-2fa', component: Enable2FAComponent },
      { path: 'security', component: SecuritySettingsComponent },
      { path: 'patients', component: PatientListComponent },
      { path: 'patients/new', component: PatientFormComponent },
      { path: 'patients/:id/edit', component: PatientFormComponent },
      { path: 'patients/:patientId/records/new', component: MedicalRecordFormComponent },
      { path: 'appointments', component: AppointmentCalendarComponent },
       { path: 'password-policy', component: PasswordPolicyComponent },
          { path: 'session-playback', component: SessionPlaybackComponent },
          { path: 'threat-hunting', component: ThreatHuntingComponent },
          { path: 'blockchain-explorer', component: BlockchainExplorerComponent },
    ]
  },
  {
    path: 'security',
  canActivate: [RoleGuard],
  data: { roles: ['Admin', 'Security'] },
  children: [
    { path: '', component: SecurityDashboardComponent },
    { path: 'webauthn-setup', component: WebAuthnSetupComponent },
    { path: 'trusted-devices', component: TrustedDevicesComponent },
  }
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }