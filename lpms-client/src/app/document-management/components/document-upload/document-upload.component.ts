import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, BehaviorSubject, Subscription, of, throwError } from 'rxjs';
import { catchError, map, tap, finalize } from 'rxjs/operators';

import { DocumentService } from '../../services/document.service';
import { DocumentCategory, AccessLevel, DocumentType, Document, DocumentUpload } from '../../models/document.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.css']
})
export class DocumentUploadComponent {
  selectedFile: File | null = null;
  documentType: DocumentType = DocumentType.OTHER;
  category: DocumentCategory = DocumentCategory.OTHER;
  accessLevel: AccessLevel = AccessLevel.PRIVATE;
  caseId: string = '';
  description: string = '';
  tags: string = '';
  
  // Expose enums to template
  AccessLevel = AccessLevel;
  DocumentCategory = DocumentCategory;
  DocumentType = DocumentType;

  isUploading = false;
  uploadProgress = 0;
  dragOver = false;

  // Available options for dropdowns
  documentTypes = Object.values(DocumentType);
  categories = Object.values(DocumentCategory);
  accessLevels = Object.values(AccessLevel);
  
  // Upload state
  private uploadSubscription: Subscription | null = null;

  // Allowed file types
  allowedTypes = [
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'image/jpeg',
    'image/png',
    'image/gif',
    'text/plain'
  ];

  maxFileSize = 10 * 1024 * 1024; // 10MB

  constructor(
    private documentService: DocumentService,
    private router: Router,
    private http: HttpClient
  ) {}

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    this.validateAndSetFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.validateAndSetFile(files[0]);
    }
  }

  validateAndSetFile(file: File): void {
    // Check file type
    if (!this.allowedTypes.includes(file.type)) {
      alert('Invalid file type. Please upload PDF, Word, Excel, or image files.');
      return;
    }

    // Check file size
    if (file.size > this.maxFileSize) {
      alert(`File size exceeds ${this.formatFileSize(this.maxFileSize)}. Please upload a smaller file.`);
      return;
    }

    this.selectedFile = file;
  }

  removeFile(): void {
    this.selectedFile = null;
  }

  uploadDocument(): void {
    if (!this.selectedFile) {
      alert('Please select a file to upload.');
      return;
    }

    if (!this.category) {
      alert('Please select a document category.');
      return;
    }

    this.isUploading = true;
    this.uploadProgress = 0;

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('documentType', this.documentType.toString());
    formData.append('category', this.category.toString());
    formData.append('accessLevel', this.accessLevel.toString());
    
    if (this.caseId) {
      formData.append('caseId', this.caseId);
    }
    if (this.description) {
      formData.append('description', this.description);
    }
    if (this.tags) {
      formData.append('tags', JSON.stringify(this.tags.split(',').map(t => t.trim())));
    }

    // Cancel any existing upload
    if (this.uploadSubscription) {
      this.uploadSubscription.unsubscribe();
    }

    this.uploadSubscription = this.http.post<Document>(
      `${environment.apiBaseUrl}/api/documents/upload`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
      }
    ).pipe(
      tap(event => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round(100 * event.loaded / event.total);
        }
      }),
      catchError(error => {
        console.error('Upload error:', error);
        return throwError(() => new Error('Upload failed. Please try again.'));
      }),
      finalize(() => {
        this.isUploading = false;
      })
    ).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.Response) {
          // Upload complete
          this.uploadProgress = 100;
          this.router.navigate(['/document-management']);
        }
      },
      error: (error) => {
        console.error('Upload failed:', error);
        alert(error.message || 'Failed to upload document. Please try again.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/document-management']);
  }

  formatFileSize(bytes: number): string {
    return this.documentService.formatFileSize(bytes);
  }

  getFileIcon(file: File): string {
    if (file.type.includes('pdf')) return '📄';
    if (file.type.includes('word')) return '📝';
    if (file.type.includes('excel') || file.type.includes('spreadsheet')) return '📊';
    if (file.type.includes('image')) return '🖼️';
    return '📎';
  }

  // ✅ Helper method to determine document type
  // Helper to process upload events with proper typing
  private getUploadEventMessage(event: HttpEvent<any>) {
    if (event.type === HttpEventType.UploadProgress && event.total) {
      return { 
        type: 'progress' as const, 
        loaded: event.loaded, 
        total: event.total 
      };
    } else if (event.type === HttpEventType.Response) {
      return { 
        type: 'complete' as const, 
        response: event.body 
      };
    }
    return { 
      type: 'unknown' as const, 
      event 
    };
  }

  // Clean up subscriptions
  ngOnDestroy() {
    if (this.uploadSubscription) {
      this.uploadSubscription.unsubscribe();
    }
  }

  private getDocumentTypeFromFile(file: File): DocumentType {
    const ext = file.name.split('.').pop()?.toLowerCase() || '';
    const mimeType = file.type.toLowerCase();

    if (mimeType.includes('pdf') || ext === 'pdf') return DocumentType.OTHER;
    if (mimeType.includes('word') || ext === 'doc' || ext === 'docx') return DocumentType.MEMO;
    if (ext === 'contract' || file.name.toLowerCase().includes('contract')) return DocumentType.CONTRACT;
    if (ext === 'brief' || file.name.toLowerCase().includes('brief')) return DocumentType.BRIEF;
    return DocumentType.OTHER;
  }
}
