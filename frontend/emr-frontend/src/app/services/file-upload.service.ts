// file-upload.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface UploadProgress {
  progress: number;
  file?: any;
}

@Injectable({
  providedIn: 'root'
})
export class FileUploadService {
  private apiUrl = 'http://localhost:5000/api/medicaldocuments';

  constructor(private http: HttpClient) {}

  uploadDocument(
    medicalRecordId: number,
    file: File,
    description: string
  ): Observable<UploadProgress> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('description', description);

    return this.http.post<any>(
      `${this.apiUrl}/upload/${medicalRecordId}`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
      }
    ).pipe(
      map(event => this.getProgress(event))
    );
  }

  getDocumentsByRecordId(recordId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/record/${recordId}`);
  }

  downloadDocument(documentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${documentId}`, {
      responseType: 'blob'
    });
  }

  deleteDocument(documentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${documentId}`);
  }

  private getProgress(event: HttpEvent<any>): UploadProgress {
    switch (event.type) {
      case HttpEventType.UploadProgress:
        const progress = event.total
          ? Math.round((100 * event.loaded) / event.total)
          : 0;
        return { progress };

      case HttpEventType.Response:
        return { progress: 100, file: event.body };

      default:
        return { progress: 0 };
    }
  }
}