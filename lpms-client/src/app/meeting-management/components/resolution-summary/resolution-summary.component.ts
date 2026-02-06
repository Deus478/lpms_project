import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { ResolutionDto, CreateResolutionDto, MinuteDto, CreateMinuteDto } from '../../models/meeting-api.model';
import { DocumentService } from '../../../document-management/services/document.service';
import { Document } from '../../../document-management/models/document.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-resolution-summary',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './resolution-summary.component.html',
  styleUrls: ['./resolution-summary.component.css']
})
export class ResolutionSummaryComponent implements OnInit {
  summary: { total: number; pending: number; inProgress: number; completed: number; overdue: number } | null = null;
  isLoading = false;
  // Meeting-scoped data
  selectedMeetingId: number | null = null;
  resolutions: ResolutionDto[] = [];
  minutes: MinuteDto[] = [];
  isLoadingRes = false;
  isLoadingMin = false;
  isPostingRes = false;
  isPostingMin = false;
  
  // Per-minute document edit state
  editMinuteDocs: Record<number, string> = {};
  minuteDocUpdating: Record<number, boolean> = {};
  minuteDocError: Record<number, string> = {};
  toasts: { id: number; type: 'success' | 'error'; text: string }[] = [];
  private nextToastId = 1;

  // Document picker state
  allDocuments: Document[] = [];
  showPicker: Record<number, boolean> = {};
  docSearch: Record<number, string> = {};
  isLoadingDocs = false;
  pickerSelection: Record<number, string | null> = {};

  // New resolution form
  newResTitle = '';
  newResDescription = '';
  newResDueDate = '';
  newResResponsibleParty = '';

  // New minute form
  newMinuteContent = '';
  newMinuteDocumentId = '';

  constructor(private meetingService: MeetingService, private documentService: DocumentService) {}

  ngOnInit(): void {
    this.loadSummary();
    // Preload documents for picker
    this.documentService.getAllDocuments().subscribe(docs => this.allDocuments = docs);
  }

  loadSummary(): void {
    this.isLoading = true;
    this.meetingService.getResolutionSummary().subscribe({
      next: data => {
        this.summary = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadForMeeting(): void {
    if (!this.selectedMeetingId || this.selectedMeetingId <= 0) {
      return;
    }
    this.loadResolutions();
    this.loadMinutes();
  }

  loadResolutions(): void {
    if (!this.selectedMeetingId) return;
    this.isLoadingRes = true;
    this.meetingService.getResolutions(this.selectedMeetingId).subscribe({
      next: list => {
        this.resolutions = list;
        this.isLoadingRes = false;
      },
      error: () => {
        this.isLoadingRes = false;
      }
    });
  }

  addResolution(): void {
    if (!this.selectedMeetingId || !this.newResTitle.trim()) return;
    const dto: CreateResolutionDto = {
      title: this.newResTitle.trim(),
      description: this.newResDescription || undefined,
      dueDate: this.newResDueDate ? new Date(this.newResDueDate).toISOString() : undefined,
      responsibleParty: this.newResResponsibleParty || undefined
    };
    this.isPostingRes = true;
    this.meetingService.addResolution(this.selectedMeetingId, dto).subscribe({
      next: res => {
        this.resolutions = [...this.resolutions, res];
        this.isPostingRes = false;
        this.newResTitle = '';
        this.newResDescription = '';
        this.newResDueDate = '';
        this.newResResponsibleParty = '';
      },
      error: () => {
        this.isPostingRes = false;
      }
    });
  }

  loadMinutes(): void {
    if (!this.selectedMeetingId) return;
    this.isLoadingMin = true;
    this.meetingService.getMinutes(this.selectedMeetingId).subscribe({
      next: list => {
        this.minutes = list;
        // Initialize editable document inputs with current values
        this.editMinuteDocs = {};
        for (const m of list) {
          this.editMinuteDocs[m.minuteId] = m.documentId || '';
        }
        this.isLoadingMin = false;
      },
      error: () => {
        this.isLoadingMin = false;
      }
    });
  }

  addMinute(): void {
    if (!this.selectedMeetingId || !this.newMinuteContent.trim()) return;
    const trimmedId = (this.newMinuteDocumentId || '').trim();
    const dto: CreateMinuteDto = {
      content: this.newMinuteContent.trim(),
      documentId: trimmedId && this.isGuid(trimmedId) ? trimmedId : undefined
    };
    this.isPostingMin = true;
    this.meetingService.addMinute(this.selectedMeetingId, dto).subscribe({
      next: minute => {
        this.minutes = [...this.minutes, minute];
        this.isPostingMin = false;
        this.newMinuteContent = '';
        this.newMinuteDocumentId = '';
      },
      error: () => {
        this.isPostingMin = false;
      }
    });
  }

  formatDate(value: string | null | undefined): string {
    if (!value) return '';
    return this.meetingService.formatDate(value);
  }

  displayResolutionStatus(status: string | null | undefined): string {
    if (!status) return '';
    if (status === 'InProgress') return 'In Progress';
    return status;
  }

  isMinuteUpdating(minuteId: number): boolean {
    return !!this.minuteDocUpdating[minuteId];
  }

  onMinuteDocInputChange(minute: MinuteDto, value: string): void {
    const v = (value || '').trim();
    if (!v) {
      delete this.minuteDocError[minute.minuteId];
      return;
    }
    if (!this.isGuid(v)) {
      this.minuteDocError[minute.minuteId] = 'Invalid GUID format';
    } else {
      delete this.minuteDocError[minute.minuteId];
    }
  }

  autoSaveIfChanged(minute: MinuteDto): void {
    if (!this.selectedMeetingId) return;
    if (this.showPicker[minute.minuteId]) return; // avoid blur save while picker is open
    if (this.minuteDocUpdating[minute.minuteId]) return; // avoid double-save
    const input = (this.editMinuteDocs[minute.minuteId] || '').trim();
    const current = (minute.documentId || '').trim();
    if (!input) {
      // do not auto-clear on blur; use explicit Clear button
      return;
    }
    if (!this.isGuid(input)) {
      this.minuteDocError[minute.minuteId] = 'Invalid GUID format';
      return;
    }
    if (input === current) return;
    this.saveMinuteDocument(minute);
  }

  saveMinuteDocument(minute: MinuteDto): void {
    if (!this.selectedMeetingId) return;
    if (this.minuteDocUpdating[minute.minuteId]) return;
    const input = (this.editMinuteDocs[minute.minuteId] || '').trim();
    let docId: string | null = null;
    if (input) {
      if (!this.isGuid(input)) {
        this.minuteDocError[minute.minuteId] = 'Invalid GUID format';
        return;
      }
      docId = input;
    }
    delete this.minuteDocError[minute.minuteId];
    this.minuteDocUpdating[minute.minuteId] = true;
    this.meetingService.updateMinuteDocument(this.selectedMeetingId, minute.minuteId, { documentId: docId }).pipe(
      finalize(() => {
        this.minuteDocUpdating[minute.minuteId] = false;
      })
    ).subscribe({
      next: () => {
        this.minutes = this.minutes.map(m => m.minuteId === minute.minuteId ? { ...m, documentId: docId || undefined } : m);
        this.editMinuteDocs[minute.minuteId] = '';
        this.showToast('success', 'Document link saved');
      },
      error: (err) => {
        console.error('Failed to save document link', err);
        this.showToast('error', 'Failed to save document link');
      }
    });
  }

  deleteDocument(minute: MinuteDto): void {
    if (!this.selectedMeetingId) return;
    
    if (confirm('Are you sure you want to delete this document link? This will not delete the actual document.')) {
      this.minuteDocUpdating[minute.minuteId] = true;
      this.meetingService.updateMinuteDocument(this.selectedMeetingId, minute.minuteId, { documentId: null }).pipe(
        finalize(() => {
          this.minuteDocUpdating[minute.minuteId] = false;
        })
      ).subscribe({
        next: () => {
          this.minutes = this.minutes.map(m => 
            m.minuteId === minute.minuteId ? { ...m, documentId: undefined } : m
          );
          this.editMinuteDocs[minute.minuteId] = '';
          this.showToast('success', 'Document link deleted');
        },
        error: (err) => {
          console.error('Failed to delete document link', err);
          this.showToast('error', 'Failed to delete document link');
        }
      });
    }
  }

  confirmDeleteDocument(minute: MinuteDto): void {
    if (confirm('Are you sure you want to remove the link to this document? The document itself will not be deleted.')) {
      this.deleteDocument(minute);
    }
  }

  clearMinuteDocument(minute: MinuteDto): void {
    if (!this.selectedMeetingId) return;
    if (this.minuteDocUpdating[minute.minuteId]) return;
    this.editMinuteDocs[minute.minuteId] = '';
    delete this.minuteDocError[minute.minuteId];
    this.minuteDocUpdating[minute.minuteId] = true;
    this.meetingService.updateMinuteDocument(this.selectedMeetingId, minute.minuteId, { documentId: null }).pipe(
      finalize(() => {
        this.minuteDocUpdating[minute.minuteId] = false;
      })
    ).subscribe({
      next: () => {
        this.minutes = this.minutes.map(m => m.minuteId === minute.minuteId ? { ...m, documentId: undefined } : m);
        this.showToast('success', 'Document link cleared');
      },
      error: (err) => {
        console.error('Failed to clear document link', err);
        this.showToast('error', 'Failed to clear document link');
      }
    });
  }

  // ----- Document picker helpers -----
  openPicker(minute: MinuteDto): void {
    this.showPicker[minute.minuteId] = true;
    this.pickerSelection[minute.minuteId] = this.isGuid(minute.documentId || '') ? (minute.documentId as string) : null;
    if (this.allDocuments.length === 0) {
      this.isLoadingDocs = true;
      this.documentService.getAllDocuments().subscribe({
        next: docs => { this.allDocuments = docs; this.isLoadingDocs = false; },
        error: () => { this.isLoadingDocs = false; }
      });
    }
  }

  closePicker(minute: MinuteDto): void {
    this.showPicker[minute.minuteId] = false;
  }

  getFilteredDocuments(minuteId: number): Document[] {
    const term = (this.docSearch[minuteId] || '').toLowerCase();
    const source = this.allDocuments.filter(d => this.isGuid(d.id));
    if (!term) return source;
    return source.filter(d =>
      d.originalName?.toLowerCase().includes(term) ||
      d.description?.toLowerCase().includes(term) ||
      d.id.toLowerCase().includes(term)
    );
  }

  selectDoc(minute: MinuteDto, docId: string): void {
    this.pickerSelection[minute.minuteId] = docId;
  }

  attachSelected(minute: MinuteDto): void {
    const selected = this.pickerSelection[minute.minuteId];
    if (!selected) return;
    if (!this.isGuid(selected)) {
      this.minuteDocError[minute.minuteId] = 'Selected document has an invalid ID (not a GUID)';
      this.showToast('error', 'Selected document cannot be linked');
      return;
    }
    this.editMinuteDocs[minute.minuteId] = selected;
    delete this.minuteDocError[minute.minuteId];
    this.showPicker[minute.minuteId] = false;
    this.saveMinuteDocument(minute);
  }

  pickDocument(minute: MinuteDto, doc: Document): void {
    // Guard against non-GUID ids (e.g., mock data fallback)
    if (!this.isGuid(doc.id)) {
      this.editMinuteDocs[minute.minuteId] = doc.id;
      this.minuteDocError[minute.minuteId] = 'Selected document has an invalid ID (not a GUID)';
      this.showToast('error', 'Selected document cannot be linked');
      this.showPicker[minute.minuteId] = false;
      return;
    }
    this.editMinuteDocs[minute.minuteId] = doc.id;
    delete this.minuteDocError[minute.minuteId];
    this.showPicker[minute.minuteId] = false; // close first to avoid blur auto-save overlap
    this.saveMinuteDocument(minute);
  }

  private showToast(type: 'success' | 'error', text: string): void {
    const id = this.nextToastId++;
    this.toasts = [...this.toasts, { id, type, text }];
    setTimeout(() => {
      this.toasts = this.toasts.filter(t => t.id !== id);
    }, 3000);
  }

  private isGuid(str: string): boolean {
    return /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/.test(str);
  }
}
