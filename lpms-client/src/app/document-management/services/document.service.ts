// src/app/document-management/services/document.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { tap, map, catchError } from 'rxjs/operators';
import { Document, DocumentUpload, DocumentFilter, DocumentType, DocumentStatus } from '../models/document.model';
import { environment } from '../../../environments/environment';

interface DocumentResponseDto {
  id: string;
  fileName?: string | null;
  fileExtension?: string | null;
  fileSizeBytes: number;
  description?: string | null;
  uploadedBy?: string | null;
  uploadedDate: string;
  isArchived: boolean;
  documentType?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/documents`;
  private documentsSubject = new BehaviorSubject<Document[]>([]);
  public documents$ = this.documentsSubject.asObservable();

  // Mock data for development
  private mockDocuments: Document[] = [
    {
      id: '1',
      fileName: 'contract_001.pdf',
      originalName: 'Employment Contract - John Doe.pdf',
      fileSize: 245678,
      mimeType: 'application/pdf',
      documentType: DocumentType.CONTRACT,
      status: DocumentStatus.APPROVED,
      caseId: '1',
      uploadedBy: 'admin@law.com',
      uploadedAt: new Date('2024-01-15'),
      lastModified: new Date('2024-01-15'),
      description: 'Employment contract for John Doe',
      tags: ['employment', 'contract', '2024'],
      version: 1,
      isArchived: false,
      accessLevel: 'CONFIDENTIAL'
    },
    {
      id: '2',
      fileName: 'brief_002.pdf',
      originalName: 'Legal Brief - Smith vs Jones.pdf',
      fileSize: 567890,
      mimeType: 'application/pdf',
      documentType: DocumentType.BRIEF,
      status: DocumentStatus.UNDER_REVIEW,
      caseId: '2',
      uploadedBy: 'lawyer@law.com',
      uploadedAt: new Date('2024-02-20'),
      lastModified: new Date('2024-02-22'),
      description: 'Legal brief for Smith vs Jones case',
      tags: ['litigation', 'brief', 'civil'],
      version: 2,
      isArchived: false,
      accessLevel: 'PRIVATE'
    },
    {
      id: '3',
      fileName: 'archived_003.pdf',
      originalName: 'Archived Contract.pdf',
      fileSize: 123456,
      mimeType: 'application/pdf',
      documentType: DocumentType.CONTRACT,
      status: DocumentStatus.ARCHIVED,
      uploadedBy: 'admin@law.com',
      uploadedAt: new Date('2023-12-01'),
      lastModified: new Date('2023-12-01'),
      description: 'Old archived contract',
      tags: ['archived', 'contract'],
      version: 1,
      isArchived: true,
      archivedAt: new Date('2024-01-01'),
      accessLevel: 'PRIVATE'
    }
  ];

  constructor(private http: HttpClient) {
    this.documentsSubject.next(this.mockDocuments);
  }

  // Get all documents (alias for compatibility)
  getDocuments(): Observable<Document[]> {
    return this.getAllDocuments();
  }

  // Get all documents
  getAllDocuments(): Observable<Document[]> {
    return this.http.get<DocumentResponseDto[]>(this.apiUrl).pipe(
      map(dtos => dtos.map(dto => this.mapDtoToDocument(dto))),
      tap(docs => this.documentsSubject.next(docs)),
      catchError(err => {
        console.error('Error loading documents from API, falling back to mock data', err);
        this.documentsSubject.next(this.mockDocuments);
        return of(this.mockDocuments);
      })
    );
  }

  // Get archived documents
  getArchivedDocuments(): Observable<Document[]> {
    return this.http.get<DocumentResponseDto[]>(`${this.apiUrl}?includeArchived=true`).pipe(
      map(dtos => dtos
        .filter(dto => dto.isArchived)
        .map(dto => this.mapDtoToDocument(dto))
      ),
      catchError(err => {
        console.error('Error loading archived documents from API, falling back to mock data', err);
        return of(this.mockDocuments.filter(d => d.isArchived));
      })
    );
  }

  // Get document by ID
  getDocumentById(id: string): Observable<Document> {
    return this.http.get<DocumentResponseDto>(`${this.apiUrl}/${id}`).pipe(
      map(dto => this.mapDtoToDocument(dto)),
      catchError(err => throwError(() => err))
    );
  }

  // Get document statistics (computed client-side from latest list)
  getDocumentStats(): Observable<{
    total: number;
    byType: { [key: string]: number };
    byStatus: { [key: string]: number };
    archived: number;
  }> {
    return this.getAllDocuments().pipe(
      map(docs => {
        const stats = {
          total: docs.length,
          byType: {} as { [key: string]: number },
          byStatus: {} as { [key: string]: number },
          archived: docs.filter(d => d.isArchived).length
        };

        docs.forEach(doc => {
          stats.byType[doc.documentType] = (stats.byType[doc.documentType] || 0) + 1;
          stats.byStatus[doc.status] = (stats.byStatus[doc.status] || 0) + 1;
        });

        return stats;
      })
    );
  }

  // Filter documents (client-side, based on cached list)
  filterDocuments(filter: DocumentFilter): Observable<Document[]> {
    const source = this.documentsSubject.value.length
      ? this.documentsSubject.value
      : this.mockDocuments;

    let filtered = [...source];

    if (filter.documentType) {
      filtered = filtered.filter(d => d.documentType === filter.documentType);
    }
    if (filter.status) {
      filtered = filtered.filter(d => d.status === filter.status);
    }
    if (filter.caseId) {
      filtered = filtered.filter(d => d.caseId === filter.caseId);
    }
    if (filter.isArchived !== undefined) {
      filtered = filtered.filter(d => d.isArchived === filter.isArchived);
    }
    if (filter.searchTerm) {
      const term = filter.searchTerm.toLowerCase();
      filtered = filtered.filter(d => 
        d.originalName.toLowerCase().includes(term) ||
        d.description?.toLowerCase().includes(term) ||
        d.tags?.some(tag => tag.toLowerCase().includes(term))
      );
    }

    return of(filtered);
  }

  // Upload document with progress tracking (wired to backend)
  uploadDocument(upload: DocumentUpload): Observable<{ progress: number; document?: Document }> {
    const formData = new FormData();
    formData.append('file', upload.file);
    formData.append('title', upload.file.name);
    if (upload.description) {
      formData.append('description', upload.description);
    }
    if (upload.category) {
      formData.append('category', upload.category);
    }

    return this.http.post<DocumentResponseDto>(`${this.apiUrl}/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      map((event: HttpEvent<DocumentResponseDto>) => {
        if (event.type === HttpEventType.UploadProgress) {
          const total = event.total ?? 0;
          const progress = total > 0 ? Math.round((event.loaded / total) * 100) : 0;
          return { progress };
        }

        if (event.type === HttpEventType.Response && event.body) {
          const doc = this.mapDtoToDocument(event.body);
          const current = this.documentsSubject.value;
          this.documentsSubject.next([...current, doc]);
          return { progress: 100, document: doc };
        }

        return { progress: 0 };
      }),
      catchError(err => throwError(() => err))
    );
  }

  // Create document
  createDocument(doc: Partial<Document>): Observable<Document> {
    const newDoc: Document = {
      id: Date.now().toString(),
      fileName: doc.fileName || 'unnamed.pdf',
      originalName: doc.originalName || 'Unnamed Document',
      fileSize: doc.fileSize || 0,
      mimeType: doc.mimeType || 'application/pdf',
      documentType: doc.documentType || DocumentType.OTHER,
      status: doc.status || DocumentStatus.DRAFT,
      uploadedBy: doc.uploadedBy || 'current-user@law.com',
      uploadedAt: new Date(),
      lastModified: new Date(),
      version: 1,
      isArchived: false,
      accessLevel: doc.accessLevel || 'PRIVATE',
      caseId: doc.caseId,
      description: doc.description,
      tags: doc.tags
    };

    this.mockDocuments.push(newDoc);
    this.documentsSubject.next(this.mockDocuments);
    return of(newDoc);
  }

  // Download document
  downloadDocument(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, { responseType: 'blob' }).pipe(
      catchError(err => throwError(() => err))
    );
  }

  // Archive document
  archiveDocument(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/archive`, null).pipe(
      catchError(err => throwError(() => err))
    );
  }

  // Restore archived document
  restoreDocument(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/restore`, null).pipe(
      catchError(err => throwError(() => err))
    );
  }

  // Delete document permanently
  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  // Alias for permanent delete (for compatibility)
  permanentDeleteDocument(id: string): Observable<void> {
    return this.deleteDocument(id);
  }

  // Update document metadata
  updateDocument(id: string, updates: Partial<Document>): Observable<Document> {
    const doc = this.mockDocuments.find(d => d.id === id);
    if (doc) {
      Object.assign(doc, updates);
      doc.lastModified = new Date();
      this.documentsSubject.next(this.mockDocuments);
      return of(doc);
    }
    return throwError(() => new Error('Document not found'));
  }

  private mapDtoToDocument(dto: DocumentResponseDto): Document {
    const uploadedAt = new Date(dto.uploadedDate);
    const lastModified = uploadedAt;

    return {
      id: dto.id,
      fileName: dto.fileName ?? 'document',
      originalName: dto.fileName ?? 'document',
      fileSize: dto.fileSizeBytes,
      mimeType: dto.fileExtension ? this.mapExtensionToMime(dto.fileExtension) : 'application/octet-stream',
      documentType: this.mapDocumentType(dto.documentType),
      status: dto.isArchived ? DocumentStatus.ARCHIVED : DocumentStatus.APPROVED,
      caseId: undefined,
      uploadedBy: dto.uploadedBy ?? '',
      uploadedAt,
      lastModified,
      description: dto.description ?? '',
      tags: [],
      version: 1,
      isArchived: dto.isArchived,
      archivedAt: dto.isArchived ? uploadedAt : undefined,
      accessLevel: 'PRIVATE',
      downloadUrl: undefined,
      category: undefined,
      isEncrypted: false,
      checksum: undefined,
      archivedBy: undefined,
      lastModifiedBy: undefined,
      lastModifiedAt: lastModified
    };
  }

  private mapDocumentType(type?: string | null): DocumentType {
    if (!type) {
      return DocumentType.OTHER;
    }
    const normalized = type.toLowerCase();
    if (normalized.includes('contract')) return DocumentType.CONTRACT;
    if (normalized.includes('brief')) return DocumentType.BRIEF;
    if (normalized.includes('evidence')) return DocumentType.EVIDENCE;
    if (normalized.includes('correspondence')) return DocumentType.CORRESPONDENCE;
    if (normalized.includes('pleading')) return DocumentType.PLEADING;
    if (normalized.includes('memo')) return DocumentType.MEMO;
    if (normalized.includes('agreement')) return DocumentType.AGREEMENT;
    if (normalized.includes('order')) return DocumentType.COURT_ORDER;
    return DocumentType.OTHER;
  }

  private mapExtensionToMime(ext: string): string {
    const lower = ext.toLowerCase();
    if (lower === '.pdf') return 'application/pdf';
    if (lower === '.doc' || lower === '.docx') return 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
    if (lower === '.xls' || lower === '.xlsx') return 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    if (lower === '.jpg' || lower === '.jpeg') return 'image/jpeg';
    if (lower === '.png') return 'image/png';
    if (lower === '.gif') return 'image/gif';
    if (lower === '.txt') return 'text/plain';
    return 'application/octet-stream';
  }

  // Format file size helper
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}