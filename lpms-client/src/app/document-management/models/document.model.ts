// src/app/document-management/models/document.model.ts

export enum DocumentType {
  CONTRACT = 'CONTRACT',
  PLEADING = 'PLEADING',
  BRIEF = 'BRIEF',
  EVIDENCE = 'EVIDENCE',
  CORRESPONDENCE = 'CORRESPONDENCE',
  COURT_ORDER = 'COURT_ORDER',
  AGREEMENT = 'AGREEMENT',
  MEMO = 'MEMO',
  OTHER = 'OTHER'
}

export enum DocumentStatus {
  DRAFT = 'DRAFT',
  UNDER_REVIEW = 'UNDER_REVIEW',
  APPROVED = 'APPROVED',
  ARCHIVED = 'ARCHIVED',
  DELETED = 'DELETED'
}

// Document categories used throughout the application
export enum DocumentCategory {
  LEGAL_BRIEF = 'LEGAL_BRIEF',
  CONTRACT = 'CONTRACT',
  EVIDENCE = 'EVIDENCE',
  CORRESPONDENCE = 'CORRESPONDENCE',
  COURT_FILING = 'COURT_FILING',
  PLEADING = 'PLEADING',
  MEMO = 'MEMO',
  AGREEMENT = 'AGREEMENT',
  COURT_ORDER = 'COURT_ORDER',
  INTERNAL_MEMO = 'INTERNAL_MEMO',
  CLIENT_DOCUMENT = 'CLIENT_DOCUMENT',
  OTHER = 'OTHER',
  BRIEF = 'BRIEF',
  COURT_DOCUMENT = 'COURT_DOCUMENT',
  DISCOVERY_DOCUMENT = 'DISCOVERY_DOCUMENT',
  EXPERT_REPORT = 'EXPERT_REPORT',
  MEDICAL_RECORD = 'MEDICAL_RECORD',
  WITNESS_STATEMENT = 'WITNESS_STATEMENT'
}

// Added AccessLevel enum with all values your components use
export enum AccessLevel {
  PUBLIC = 'PUBLIC',
  PRIVATE = 'PRIVATE',
  INTERNAL = 'INTERNAL',
  CONFIDENTIAL = 'CONFIDENTIAL',
  RESTRICTED = 'RESTRICTED'
}

export interface Document {
  id: string;
  fileName: string;
  originalName: string;
  fileSize: number;
  mimeType: string;
  documentType: DocumentType;
  status: DocumentStatus;
  caseId?: string;
  uploadedBy: string;
  uploadedAt: Date;
  lastModified: Date;
  description?: string;
  tags?: string[];
  version: number;
  isArchived: boolean;
  archivedAt?: Date;
  accessLevel: AccessLevel;
  downloadUrl?: string;
  
  // Added missing properties that components expect
  category?: DocumentCategory;
  isEncrypted?: boolean;
  checksum?: string;
  archivedBy?: string;
  lastModifiedBy?: string;
  lastModifiedAt?: Date;
}

export interface DocumentUpload {
  file: File;
  documentType: DocumentType;
  caseId?: string;
  description?: string;
  tags?: string[];
  accessLevel: AccessLevel;
  
  // Added category field that upload component uses
  category?: DocumentCategory;
}

export interface DocumentFilter {
  documentType?: DocumentType;
  status?: DocumentStatus;
  caseId?: string;
  uploadedBy?: string;
  startDate?: Date;
  endDate?: Date;
  isArchived?: boolean;
  searchTerm?: string;
  
  // Added category filter
  category?: DocumentCategory;
}

// Optional: Export a class implementation if needed
export class DocumentModel implements Document {
  id: string;
  fileName: string;
  originalName: string;
  fileSize: number;
  mimeType: string;
  documentType: DocumentType;
  status: DocumentStatus;
  caseId?: string;
  uploadedBy: string;
  uploadedAt: Date;
  lastModified: Date;
  description?: string;
  tags?: string[];
  version: number;
  isArchived: boolean;
  archivedAt?: Date;
  accessLevel: AccessLevel;
  downloadUrl?: string;
  category?: DocumentCategory;
  isEncrypted?: boolean;
  checksum?: string;
  archivedBy?: string;
  lastModifiedBy?: string;
  lastModifiedAt?: Date;

  constructor(data: Partial<Document>) {
    this.id = data.id || '';
    this.fileName = data.fileName || '';
    this.originalName = data.originalName || '';
    this.fileSize = data.fileSize || 0;
    this.mimeType = data.mimeType || '';
    this.documentType = data.documentType || DocumentType.OTHER;
    this.status = data.status || DocumentStatus.DRAFT;
    this.caseId = data.caseId;
    this.uploadedBy = data.uploadedBy || '';
    this.uploadedAt = data.uploadedAt || new Date();
    this.lastModified = data.lastModified || new Date();
    this.description = data.description;
    this.tags = data.tags || [];
    this.version = data.version || 1;
    this.isArchived = data.isArchived || false;
    this.archivedAt = data.archivedAt;
    this.accessLevel = data.accessLevel || AccessLevel.PRIVATE;
    this.downloadUrl = data.downloadUrl;
    this.category = data.category;
    this.isEncrypted = data.isEncrypted || false;
    this.checksum = data.checksum;
    this.archivedBy = data.archivedBy;
    this.lastModifiedBy = data.lastModifiedBy;
    this.lastModifiedAt = data.lastModifiedAt;
  }
}