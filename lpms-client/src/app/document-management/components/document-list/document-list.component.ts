
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { 
  Document, 
  DocumentCategory, 
  DocumentStatus, 
  AccessLevel 
} from '../../models/document.model';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.css']
})
export class DocumentListComponent implements OnInit {
  documents: Document[] = [];
  filteredDocuments: Document[] = [];
  isLoading = false;
   DocumentCategory = DocumentCategory; // ✅ expose enum to template
  AccessLevel = AccessLevel;           // ✅ optional, but useful for template too

  
  searchTerm = '';
  selectedCategory: DocumentCategory | 'ALL' = 'ALL';
  selectedStatus: DocumentStatus | 'ALL' = 'ALL';
  selectedAccessLevel: AccessLevel | 'ALL' = 'ALL';
  
  categories = Object.values(DocumentCategory);
  statuses = Object.values(DocumentStatus);
  accessLevels = Object.values(AccessLevel);
  
  stats: any = null;

  constructor(
    private documentService: DocumentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
    this.loadStats();
  }

  loadDocuments(): void {
    this.isLoading = true;
    this.documentService.getDocuments().subscribe({
      next: (docs) => {
        this.documents = docs.filter(d => d.status !== DocumentStatus.DELETED);
        this.filteredDocuments = this.documents;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading documents:', error);
        this.isLoading = false;
      }
    });
  }

  loadStats(): void {
    this.documentService.getDocumentStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error loading stats:', error);
      }
    });
  }

  applyFilters(): void {
    this.filteredDocuments = this.documents.filter(doc => {
      const matchesSearch = !this.searchTerm || 
  doc.fileName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
  doc.description?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
  (doc.tags && doc.tags.some(tag => tag.toLowerCase().includes(this.searchTerm.toLowerCase())));

      const matchesCategory = this.selectedCategory === 'ALL' || 
        doc.category === this.selectedCategory;

      const matchesStatus = this.selectedStatus === 'ALL' || 
        doc.status === this.selectedStatus;

      const matchesAccessLevel = this.selectedAccessLevel === 'ALL' || 
        doc.accessLevel === this.selectedAccessLevel;

      return matchesSearch && matchesCategory && matchesStatus && matchesAccessLevel;
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  viewDocument(docId: string): void {
    this.router.navigate(['/document-management/view', docId]);
  }

  downloadDocument(doc: Document): void {
    this.documentService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.originalName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading document:', error);
        alert('Failed to download document');
      }
    });
  }

  archiveDocument(docId: string): void {
    if (confirm('Are you sure you want to archive this document?')) {
      this.documentService.archiveDocument(docId).subscribe({
        next: () => {
          this.loadDocuments();
          this.loadStats();
        },
        error: (error) => {
          console.error('Error archiving document:', error);
          alert('Failed to archive document');
        }
      });
    }
  }

  deleteDocument(docId: string): void {
    if (confirm('Are you sure you want to delete this document? This action cannot be undone.')) {
      this.documentService.deleteDocument(docId).subscribe({
        next: () => {
          this.loadDocuments();
          this.loadStats();
        },
        error: (error) => {
          console.error('Error deleting document:', error);
          alert('Failed to delete document');
        }
      });
    }
  }

  uploadDocument(): void {
    this.router.navigate(['/document-management/upload']);
  }

  viewArchive(): void {
    this.router.navigate(['/document-management/archive']);
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