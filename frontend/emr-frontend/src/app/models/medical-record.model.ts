// medical-record.model.ts
export interface MedicalRecord {
  id: number;
  patientId: number;
  patientName: string;
  doctorId: number;
  doctorName: string;
  visitDate: Date;
  chiefComplaint: string;
  presentIllness: string;
  physicalExamination: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  vitalSigns: VitalSigns;
}

export interface VitalSigns {
  temperature?: number;
  bloodPressure?: string;
  heartRate?: number;
  respiratoryRate?: number;
  weight?: number;
  height?: number;
}

export interface CreateMedicalRecordDto {
  patientId: number;
  doctorId: number;
  visitDate: Date;
  chiefComplaint: string;
  presentIllness?: string;
  physicalExamination?: string;
  diagnosis?: string;
  treatment?: string;
  notes?: string;
  vitalSigns?: VitalSigns;
}