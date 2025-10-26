// file-upload.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FileUploadService } from '../../services/file-upload.service';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent implements OnInit {
  @Input() medicalRecordId!: number;

  uploadForm: FormGroup;
  selectedFile: File | null = null;
  uploadProgress = 0;
  uploading = false;
  documents: any[] = [];

  displayedColumns = ['fileName', 'fileType', 'fileSize', 'uploadedAt', 'actions'];

  constructor(
    private fb: FormBuilder,
    private fileService: FileUploadService
  ) {
    this.uploadForm = this.fb.group({
      description: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadDocuments();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file size (max 10MB)
      if (file.size > 10 * 1024 * 1024) {
        alert('File quá lớn. Kích thước tối đa là 10MB');
        return;
      }

      // Validate file type
      const allowedTypes = [
        'image/jpeg',
        'image/png',
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
      ];

      if (!allowedTypes.includes(file.type)) {
        alert('Loại file không được hỗ trợ');
        return;
      }

      this.selectedFile = file;
    }
  }

  upload(): void {
    if (!this.selectedFile || !this.uploadForm.valid) {
      return;
    }

    this.uploading = true;
    this.uploadProgress = 0;

    this.fileService.uploadDocument(
      this.medicalRecordId,
      this.selectedFile,
      this.uploadForm.value.description
    ).subscribe(
      progress => {
        this.uploadProgress = progress.progress;

        if (progress.file) {
          this.uploading = false;
          this.selectedFile = null;
          this.uploadForm.reset();
          this.loadDocuments();
          alert('Upload thành công!');
        }
      },
      error => {
        this.uploading = false;
        console.error('Upload error:', error);
        alert('Upload thất bại!');
      }
    );
  }

  loadDocuments(): void {
    this.fileService.getDocumentsByRecordId(this.medicalRecordId).subscribe(
      data => {
        this.documents = data;
      },
      error => console.error('Error loading documents:', error)
    );
  }

  downloadDocument(doc: any): void {
    this.fileService.downloadDocument(doc.id).subscribe(
      blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error => console.error('Download error:', error)
    );
  }

  deleteDocument(doc: any): void {
    if (confirm(`Bạn có chắc muốn xóa file "${doc.fileName}"?`)) {
      this.fileService.deleteDocument(doc.id).subscribe(
        () => {
          this.loadDocuments();
          alert('Xóa thành công!');
        },
        error => {
          console.error('Delete error:', error);
          alert('Xóa thất bại!');
        }
      );
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
  }

  getFileIcon(fileType: string): string {
    switch (fileType) {
      case 'Image': return 'image';
      case 'PDF': return 'picture_as_pdf';
      default: return 'description';
    }
  }
}