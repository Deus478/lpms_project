
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { Document, DocumentStatus, DocumentCategory, AccessLevel } from '../../models/document.model';

@Component({
  selector: 'app-document-view',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './document-view.component.html',
  styleUrls: ['./document-view.component.css']
})
export class DocumentViewComponent implements OnInit {
  document?: Document;
  isLoading = false;
  documentId?: string;
 DocumentCategory = DocumentCategory; // ✅ expose enum to template
  AccessLevel = AccessLevel;           // ✅ optional, but useful for template too

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private documentService: DocumentService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.documentId = params.get('id') || undefined;
      if (this.documentId) {
        this.loadDocument(this.documentId);
      }
    });
  }

  loadDocument(id: string): void {
    this.isLoading = true;
    this.documentService.getDocumentById(id).subscribe({
      next: (doc) => {
        this.document = doc;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading document:', error);
        this.isLoading = false;
      }
    });
  }

  downloadDocument(): void {
    if (!this.document) return;

    this.documentService.downloadDocument(this.document.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = this.document!.originalName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading document:', error);
        alert('Failed to download document');
      }
    });
  }

  archiveDocument(): void {
    if (!this.documentId) return;

    if (confirm('Are you sure you want to archive this document?')) {
      this.documentService.archiveDocument(this.documentId).subscribe({
        next: () => {
          this.router.navigate(['/document-management']);
        },
        error: (error) => {
          console.error('Error archiving document:', error);
          alert('Failed to archive document');
        }
      });
    }
  }

  deleteDocument(): void {
    if (!this.documentId) return;

    if (confirm('Are you sure you want to delete this document? This action cannot be undone.')) {
      this.documentService.deleteDocument(this.documentId).subscribe({
        next: () => {
          this.router.navigate(['/document-management']);
        },
        error: (error) => {
          console.error('Error deleting document:', error);
          alert('Failed to delete document');
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/document-management']);
  }

 getCategoryClass(category: DocumentCategory | undefined): string {
  if (!category) return 'category-other';
  
  const classes: { [key in DocumentCategory]: string } = {
    [DocumentCategory.LEGAL_BRIEF]: 'category-legal-brief',
    [DocumentCategory.CONTRACT]: 'category-contract',
    [DocumentCategory.COURT_FILING]: 'category-court-filing',
    [DocumentCategory.EVIDENCE]: 'category-evidence',
    [DocumentCategory.CORRESPONDENCE]: 'category-correspondence',
    [DocumentCategory.PLEADING]: 'category-pleading',
    [DocumentCategory.MEMO]: 'category-memo',
    [DocumentCategory.AGREEMENT]: 'category-agreement',
    [DocumentCategory.COURT_ORDER]: 'category-court-order',
    [DocumentCategory.INTERNAL_MEMO]: 'category-internal-memo',
    [DocumentCategory.CLIENT_DOCUMENT]: 'category-client-document',
    [DocumentCategory.OTHER]: 'category-other'
  };
  return classes[category] || 'category-other';
}


  getStatusClass(status: DocumentStatus): string {
    const classes: { [key in DocumentStatus]: string } = {
      [DocumentStatus.DRAFT]: 'status-draft',
      [DocumentStatus.UNDER_REVIEW]: 'status-under-review',
      [DocumentStatus.APPROVED]: 'status-approved',
      [DocumentStatus.ARCHIVED]: 'status-archived',
      [DocumentStatus.DELETED]: 'status-deleted'
    };
    return classes[status];
  }

 getAccessLevelClass(accessLevel: AccessLevel | string): string {
  const classes: { [key in AccessLevel]: string } = {
    [AccessLevel.PUBLIC]: 'access-public',
    [AccessLevel.PRIVATE]: 'access-private',
    [AccessLevel.INTERNAL]: 'access-internal',
    [AccessLevel.CONFIDENTIAL]: 'access-confidential',
    [AccessLevel.RESTRICTED]: 'access-restricted'
  };
  return classes[accessLevel as AccessLevel] || 'access-private';
}
  formatFileSize(bytes: number): string {
    return this.documentService.formatFileSize(bytes);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }
}