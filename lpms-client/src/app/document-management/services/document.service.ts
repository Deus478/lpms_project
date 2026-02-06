// src/app/document-management/services/document.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { catchError, map, tap, filter, switchMap } from 'rxjs/operators';

import { 
  Document, 
  DocumentType, 
  DocumentStatus, 
  DocumentUpload, 
  DocumentFilter, 
  DocumentCategory, 
  AccessLevel 
} from '../models/document.model';

import { environment } from '../../../environments/environment';

// Interface for the API response
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
  category?: string | null;
  accessLevel?: string | null;
  status?: string | null;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/documents`;
  private documentsSubject = new BehaviorSubject<Document[]>([]);
  public documents$ = this.documentsSubject.asObservable();
  private readonly statusStoreKey = 'lpms_doc_status_overrides';

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
      accessLevel: AccessLevel.CONFIDENTIAL
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
      accessLevel: AccessLevel.PRIVATE
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
      accessLevel: AccessLevel.PRIVATE
    },
    {
      id: '4',
      fileName: 'draft_004.pdf',
      originalName: 'Draft Agreement.pdf',
      fileSize: 345678,
      mimeType: 'application/pdf',
      documentType: DocumentType.AGREEMENT,
      status: DocumentStatus.DRAFT,
      caseId: '3',
      uploadedBy: 'lawyer@law.com',
      uploadedAt: new Date('2024-03-01'),
      lastModified: new Date('2024-03-01'),
      description: 'Draft agreement waiting for review',
      tags: ['draft', 'agreement'],
      version: 1,
      isArchived: false,
      accessLevel: AccessLevel.INTERNAL
    },
    {
      id: '5',
      fileName: 'draft_005.pdf',
      originalName: 'Draft Motion.pdf',
      fileSize: 234567,
      mimeType: 'application/pdf',
      documentType: DocumentType.PLEADING,
      status: DocumentStatus.DRAFT,
      caseId: '4',
      uploadedBy: 'admin@law.com',
      uploadedAt: new Date('2024-03-10'),
      lastModified: new Date('2024-03-10'),
      description: 'Draft motion for filing',
      tags: ['draft', 'motion', 'pleading'],
      version: 1,
      isArchived: false,
      accessLevel: AccessLevel.CONFIDENTIAL
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
    // Use real API and include archived items for consistent stats with Archive page
    return this.http.get<DocumentResponseDto[]>(`${this.apiUrl}?includeArchived=true`).pipe(
      map(dtos => dtos.map(dto => this.mapDtoToDocument(dto))),
      map(docs => this.applyStatusOverrides(docs)),
      tap(docs => this.documentsSubject.next(docs)),
      catchError(err => {
        console.error('Error loading documents from API, falling back to mock data', err);
        const withOverrides = this.applyStatusOverrides(this.mockDocuments);
        this.documentsSubject.next(withOverrides);
        return of(withOverrides);
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
      map(docs => this.applyStatusOverrides(docs)),
      catchError(err => {
        console.error('Error loading archived documents from API, falling back to mock data', err);
        return of(this.applyStatusOverrides(this.mockDocuments.filter(d => d.isArchived)));
      })
    );
  }

  // Get document by ID
  getDocumentById(id: string): Observable<Document> {
    return this.http.get<DocumentResponseDto>(`${this.apiUrl}/${id}`).pipe(
      map(dto => this.mapDtoToDocument(dto)),
      map(doc => this.applyStatusOverride(doc)),
      catchError(err => throwError(() => err))
    );
  }

  // Get document statistics (computed client-side from latest list)
  getDocumentStats(): Observable<{
    total: number;
    draft: number;
    approved: number;
    archived: number;
    totalSize: number;
    byType: { [key: string]: number };
    byStatus: { [key: string]: number };
  }> {
    return this.getAllDocuments().pipe(
      map(docs => {
        const stats = {
          total: docs.length,
          draft: docs.filter(d => d.status === DocumentStatus.DRAFT).length,
          approved: docs.filter(d => d.status === DocumentStatus.APPROVED).length,
          archived: docs.filter(d => d.isArchived || d.status === DocumentStatus.ARCHIVED).length,
          totalSize: docs.reduce((sum, d) => sum + d.fileSize, 0),
          byType: {} as { [key: string]: number },
          byStatus: {} as { [key: string]: number }
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

  /**
   * Upload a document with progress tracking
   * @param upload The document upload data
   * @returns Observable with upload progress and document
   */
  uploadDocument(upload: DocumentUpload): Observable<Document> {
    const formData = new FormData();
    formData.append('file', upload.file);
    formData.append('documentType', upload.documentType.toString());
    formData.append('category', upload.category || DocumentCategory.OTHER);
    formData.append('accessLevel', upload.accessLevel);
    
    if (upload.caseId) {
      formData.append('caseId', upload.caseId);
    }
    if (upload.description) {
      formData.append('description', upload.description);
    }
    if (upload.tags && upload.tags.length > 0) {
      formData.append('tags', JSON.stringify(upload.tags));
    }

    return this.http.post<DocumentResponseDto>(`${this.apiUrl}/upload`, formData).pipe(
      map(dto => this.mapDtoToDocument(dto)),
      tap(document => {
        // Update the documents list with the newly uploaded document
        const current = this.documentsSubject.value;
        this.documentsSubject.next([...current, document]);
      }),
      catchError(error => {
        console.error('Upload failed:', error);
        return throwError(() => new Error('Failed to upload document'));
      })
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
      accessLevel: doc.accessLevel || AccessLevel.PRIVATE,
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
      tap(() => {
        // Optimistically update local cache if present
        const docs = this.documentsSubject.value;
        const idx = docs.findIndex(d => d.id === id);
        if (idx !== -1) {
          docs[idx] = { ...docs[idx], isArchived: true, archivedAt: new Date() };
          this.documentsSubject.next([...docs]);
        }
      }),
      catchError(err => throwError(() => err))
    );
  }

  // Restore archived document
  restoreDocument(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/restore`, null).pipe(
      tap(() => {
        const docs = this.documentsSubject.value;
        const idx = docs.findIndex(d => d.id === id);
        if (idx !== -1) {
          const updated = { ...docs[idx] };
          updated.isArchived = false;
          delete updated.archivedAt;
          this.documentsSubject.next([...docs.slice(0, idx), updated, ...docs.slice(idx + 1)]);
        }
      }),
      catchError(err => throwError(() => err))
    );
  }

  // Delete document permanently
  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const docs = this.documentsSubject.value.filter(d => d.id !== id);
        this.documentsSubject.next(docs);
      }),
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

  // Update status locally (manual control) and reflect in cache immediately
  updateDocumentStatus(id: string, status: DocumentStatus): Observable<Document> {
    const docs = this.documentsSubject.value;
    const idx = docs.findIndex(d => d.id === id);
    if (idx !== -1) {
      const updated: Document = { ...docs[idx], status, lastModified: new Date() };
      const next = [...docs];
      next[idx] = updated;
      this.documentsSubject.next(next);
      const mockIdx = this.mockDocuments.findIndex(d => d.id === id);
      if (mockIdx !== -1) {
        this.mockDocuments[mockIdx] = updated;
      }
      this.setStatusOverride(id, status);
      return of(updated);
    }

    // If not found in cache, fetch and insert updated copy
    return this.getDocumentById(id).pipe(
      map(doc => {
        const updated = { ...doc, status, lastModified: new Date() } as Document;
        const list = this.documentsSubject.value;
        this.documentsSubject.next([...list, updated]);
        this.setStatusOverride(id, status);
        return updated;
      })
    );
  }

  /**
   * Map a DocumentResponseDto to a Document
   */
  private mapDtoToDocument(dto: DocumentResponseDto): Document {
    // Safely map the document type
    let documentType: DocumentType = DocumentType.OTHER;
    if (dto.documentType) {
      const type = Object.values(DocumentType).find(
        t => t.toString() === dto.documentType
      );
      if (type) documentType = type;
    }

    // Safely map the category
    let category: DocumentCategory = DocumentCategory.OTHER;
    if (dto.category) {
      const cat = Object.values(DocumentCategory).find(
        c => c.toString() === dto.category
      );
      if (cat) category = cat;
    }

    // Safely map the access level
    let accessLevel: AccessLevel = AccessLevel.PRIVATE;
    if (dto.accessLevel) {
      const normalized = String(dto.accessLevel).trim().toUpperCase();
      const level = Object.values(AccessLevel).find(
        l => l.toString().toUpperCase() === normalized
      );
      if (level) accessLevel = level;
    }

    // Safely map the status (do not infer from isArchived)
    let status: DocumentStatus = DocumentStatus.APPROVED; // Default if API omits
    if (dto.status) {
      const statusEnum = Object.values(DocumentStatus).find(
        s => s.toString() === dto.status
      );
      if (statusEnum) status = statusEnum;
    }

    return {
      id: dto.id,
      fileName: dto.fileName || 'unnamed',
      originalName: dto.fileName || 'Unnamed Document',
      fileSize: dto.fileSizeBytes,
      mimeType: this.getMimeType(dto.fileExtension || ''),
      documentType,
      status,
      uploadedBy: dto.uploadedBy || 'Unknown',
      uploadedAt: new Date(dto.uploadedDate),
      lastModified: new Date(dto.uploadedDate),
      description: dto.description || undefined,
      tags: [],
      version: 1,
      isArchived: dto.isArchived || false,
      accessLevel,
      downloadUrl: `${this.apiUrl}/${dto.id}/download`,
      category
    };
  }

  // ----- Status override helpers (persisted in localStorage) -----
  private loadStatusOverrides(): Record<string, DocumentStatus> {
    try {
      const raw = localStorage.getItem(this.statusStoreKey);
      return raw ? JSON.parse(raw) : {};
    } catch {
      return {};
    }
  }

  private saveStatusOverrides(overrides: Record<string, DocumentStatus>): void {
    try {
      localStorage.setItem(this.statusStoreKey, JSON.stringify(overrides));
    } catch {
      // ignore storage errors
    }
  }

  private getStatusOverride(id: string): DocumentStatus | undefined {
    const overrides = this.loadStatusOverrides();
    return overrides[id];
  }

  private setStatusOverride(id: string, status: DocumentStatus): void {
    const overrides = this.loadStatusOverrides();
    overrides[id] = status;
    this.saveStatusOverrides(overrides);
  }

  private applyStatusOverride(doc: Document): Document {
    const override = this.getStatusOverride(doc.id);
    return override ? { ...doc, status: override } : doc;
  }

  private applyStatusOverrides(docs: Document[]): Document[] {
    if (!docs || docs.length === 0) return docs;
    const overrides = this.loadStatusOverrides();
    if (!overrides || Object.keys(overrides).length === 0) return docs;
    return docs.map(d => (overrides[d.id] ? { ...d, status: overrides[d.id] } : d));
  }

  /**
   * Get MIME type from file extension
   */
  private getMimeType(extension: string): string {
    const ext = extension.toLowerCase().replace(/^\./, '');
    const mimeTypes: Record<string, string> = {
      // Documents
      'pdf': 'application/pdf',
      'doc': 'application/msword',
      'docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'dotx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
      'xls': 'application/vnd.ms-excel',
      'xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'ppt': 'application/vnd.ms-powerpoint',
      'pptx': 'application/vnd.openxmlformats-officedocument.presentationml.presentation',
      'rtf': 'application/rtf',
      'odt': 'application/vnd.oasis.opendocument.text',
      'ods': 'application/vnd.oasis.opendocument.spreadsheet',
      'odp': 'application/vnd.oasis.opendocument.presentation',
      'csv': 'text/csv',
      'txt': 'text/plain',
      
      // Images
      'jpg': 'image/jpeg',
      'jpeg': 'image/jpeg',
      'png': 'image/png',
      'gif': 'image/gif',
      'bmp': 'image/bmp',
      'svg': 'image/svg+xml',
      'tiff': 'image/tiff',
      'webp': 'image/webp',
      
      // Archives
      'zip': 'application/zip',
      'rar': 'application/x-rar-compressed',
      '7z': 'application/x-7z-compressed',
      'tar': 'application/x-tar',
      'gz': 'application/gzip',
      
      // Audio/Video
      'mp3': 'audio/mpeg',
      'wav': 'audio/wav',
      'ogg': 'audio/ogg',
      'mp4': 'video/mp4',
      'webm': 'video/webm',
      'mov': 'video/quicktime',
      'avi': 'video/x-msvideo'
    };
    
    return mimeTypes[ext] || 'application/octet-stream';
  }
  
  /**
   * Format file size in bytes to human readable format
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), sizes.length - 1);
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}