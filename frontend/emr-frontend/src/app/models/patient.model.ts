// patient.model.ts
export interface Patient {
  id: number;
  fullName: string;
  dateOfBirth: Date;
  gender: string;
  identityCard: string;
  phoneNumber: string;
  email: string;
  address: string;
  bloodType: string;
  allergies: string;
}

export interface CreatePatientDto {
  fullName: string;
  dateOfBirth: Date;
  gender: string;
  identityCard?: string;
  phoneNumber?: string;
  email?: string;
  address?: string;
  bloodType?: string;
  allergies?: string;
}