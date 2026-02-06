import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { DocumentModel, DocumentCategory } from '../../models/document.model';

@Component({
  selector: 'app-document-archive',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './document-archive.component.html',
  styleUrls: ['./document-archive.component.css']
})
export class DocumentArchiveComponent implements OnInit {
  archivedDocuments: DocumentModel[] = [];
  filteredDocuments: DocumentModel[] = [];
  isLoading = false;
  searchTerm = '';
DocumentCategory = DocumentCategory; 
  constructor(
    private documentService: DocumentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadArchivedDocuments();
  }

  loadArchivedDocuments(): void {
    this.isLoading = true;
    this.documentService.getArchivedDocuments().subscribe({
      next: (docs: DocumentModel[]) => {
        this.archivedDocuments = docs;
        this.filteredDocuments = docs;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading archived documents:', error);
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.filteredDocuments = this.archivedDocuments.filter((doc: DocumentModel) =>
      doc.originalName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      (doc.description?.toLowerCase().includes(this.searchTerm.toLowerCase()) ?? false)
    );
  }

  restoreDocument(docId: string): void {
    if (confirm('Are you sure you want to restore this document from the archive?')) {
      this.documentService.restoreDocument(docId).subscribe({
        next: () => this.loadArchivedDocuments(),
        error: (error: any) => {
          console.error('Error restoring document:', error);
          alert('Failed to restore document');
        }
      });
    }
  }

  permanentDelete(docId: string): void {
    if (confirm('⚠️ WARNING: This will permanently delete the document. This action cannot be undone. Are you sure?')) {
      this.documentService.permanentDeleteDocument(docId).subscribe({
        next: () => this.loadArchivedDocuments(),
        error: (error: any) => {
          console.error('Error deleting document:', error);
          alert('Failed to delete document');
        }
      });
    }
  }

  downloadDocument(doc: DocumentModel): void {
    this.documentService.downloadDocument(doc.id).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.originalName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error: any) => {
        console.error('Error downloading document:', error);
        alert('Failed to download document');
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/document-management']);
  }

  getCategoryClass(category: DocumentCategory | undefined): string {
    if (!category) return 'category-other';

    const classes: Partial<Record<DocumentCategory, string>> = {
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

  formatFileSize(bytes: number): string {
    return this.documentService.formatFileSize(bytes);
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleString();
  }
}
