import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { DocumentCategory, AccessLevel, DocumentType } from '../../models/document.model';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.css']
})
export class DocumentUploadComponent {
  selectedFile: File | null = null;
  category: DocumentCategory = DocumentCategory.OTHER;
  accessLevel: AccessLevel = AccessLevel.PRIVATE; // Changed from INTERNAL
  caseId: string = '';
  description: string = '';
  tags: string = '';
  AccessLevel = AccessLevel;           // ✅ expose enum for template
  DocumentCategory = DocumentCategory; // ✅ expose if used in HTML

  isUploading = false;
  uploadProgress = 0;
  dragOver = false;

  categories = Object.values(DocumentCategory);
  accessLevels = Object.values(AccessLevel);

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
    private router: Router
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

    // Simulate upload progress
    const progressInterval = setInterval(() => {
      this.uploadProgress += 10;
      if (this.uploadProgress >= 90) {
        clearInterval(progressInterval);
      }
    }, 200);

    // ✅ Added documentType logic
    const uploadDto = {
      file: this.selectedFile,
      documentType: this.getDocumentTypeFromFile(this.selectedFile), // <-- Added line
      category: this.category,
     accessLevel: this.accessLevel as unknown as 'PUBLIC' | 'PRIVATE' | 'CONFIDENTIAL', // ✅ type cast fix
      caseId: this.caseId || undefined,
      description: this.description || undefined,
      tags: this.tags ? this.tags.split(',').map(t => t.trim()) : []
    };

    this.documentService.uploadDocument(uploadDto).subscribe({
      next: (document) => {
        clearInterval(progressInterval);
        this.uploadProgress = 100;
        setTimeout(() => {
          this.isUploading = false;
          this.router.navigate(['/document-management']);
        }, 500);
      },
      error: (error) => {
        clearInterval(progressInterval);
        console.error('Upload error:', error);
        this.isUploading = false;
        this.uploadProgress = 0;
        alert('Failed to upload document. Please try again.');
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

  // ✅ New helper method to determine document type
  private getDocumentTypeFromFile(file: File): DocumentType {
    const ext = file.name.split('.').pop()?.toLowerCase();
    const mimeType = file.type.toLowerCase();

    if (mimeType.includes('pdf') || ext === 'pdf') return DocumentType.BRIEF;
    if (mimeType.includes('word') || ext === 'doc' || ext === 'docx') return DocumentType.PLEADING;
    if (ext === 'contract') return DocumentType.CONTRACT;
    return DocumentType.OTHER;
  }
}
