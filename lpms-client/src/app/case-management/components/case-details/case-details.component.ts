import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CaseStatus, CasePriority } from '../../models/case.model';
import { CaseService } from '../../services/case.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CaseDetailDto, CreateHearingApiDto, CreateDeadlineApiDto, UpdateHearingApiDto, UpdateDeadlineApiDto } from '../../models/case-api.model';

@Component({
  standalone: true,
  selector: 'app-case-details',
  templateUrl: './case-details.component.html',
  styleUrls: ['./case-details.component.css'],
  imports: [CommonModule, FormsModule, RouterModule], // ✅ Include RouterModule for navigation
})
export class CaseDetailsComponent implements OnInit {
  case?: CaseDetailDto;
  isLoading = false;
  caseId?: string;

  newHearing = {
    date: '',
    time: '',
    location: '',
    hearingType: '',
    remarks: ''
  };

  newDeadline = {
    dueDate: '',
    description: '',
    priority: 'Medium',
    notes: ''
  };

  editingHearingId: number | null = null;
  editHearing = {
    date: '',
    time: '',
    location: '',
    hearingType: '',
    remarks: '',
    status: ''
  };

  editingDeadlineId: number | null = null;
  editDeadline = {
    dueDate: '',
    description: '',
    priority: 'Medium',
    notes: '',
    isCompleted: false
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private caseService: CaseService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.caseId = params.get('id') || undefined;
      if (this.caseId) {
        this.loadCase(this.caseId);
      }
    });
  }

  startEditHearing(hearing: any): void {
    this.editingHearingId = hearing.hearingId;
    this.editHearing = {
      date: hearing.date ? this.toDateInputValue(hearing.date) : '',
      time: hearing.time ? this.toTimeInputValue(hearing.time) : '',
      location: hearing.location || '',
      hearingType: hearing.hearingType || '',
      remarks: hearing.remarks || '',
      status: hearing.status || ''
    };
  }

  cancelEditHearing(): void {
    this.editingHearingId = null;
    this.editHearing = {
      date: '',
      time: '',
      location: '',
      hearingType: '',
      remarks: '',
      status: ''
    };
  }

  updateHearing(): void {
    if (!this.caseId || !this.editingHearingId) {
      return;
    }

    const dto: UpdateHearingApiDto = {};

    if (this.editHearing.date) {
      dto.date = new Date(this.editHearing.date).toISOString();
    }
    if (this.editHearing.time) {
      dto.time = this.formatTime(this.editHearing.time);
    }
    if (this.editHearing.location) {
      dto.location = this.editHearing.location;
    }
    if (this.editHearing.hearingType) {
      dto.hearingType = this.editHearing.hearingType;
    }
    if (this.editHearing.remarks !== '') {
      dto.remarks = this.editHearing.remarks;
    }
    if (this.editHearing.status) {
      dto.status = this.editHearing.status;
    }

    this.caseService.updateHearing(this.caseId, this.editingHearingId, dto).subscribe({
      next: updated => {
        if (this.case) {
          this.case.hearings = this.case.hearings.map(h =>
            h.hearingId === updated.hearingId ? updated : h
          );
        }
        this.cancelEditHearing();
      },
      error: error => {
        console.error('Error updating hearing:', error);
      }
    });
  }

  startEditDeadline(deadline: any): void {
    this.editingDeadlineId = deadline.deadlineId;
    this.editDeadline = {
      dueDate: deadline.dueDate ? this.toDateInputValue(deadline.dueDate) : '',
      description: deadline.description || '',
      priority: deadline.priority || 'Medium',
      notes: deadline.notes || '',
      isCompleted: !!deadline.isCompleted
    };
  }

  cancelEditDeadline(): void {
    this.editingDeadlineId = null;
    this.editDeadline = {
      dueDate: '',
      description: '',
      priority: 'Medium',
      notes: '',
      isCompleted: false
    };
  }

  updateDeadline(): void {
    if (!this.caseId || !this.editingDeadlineId) {
      return;
    }

    const dto: UpdateDeadlineApiDto = {};

    if (this.editDeadline.dueDate) {
      dto.dueDate = new Date(this.editDeadline.dueDate).toISOString();
    }
    if (this.editDeadline.description) {
      dto.description = this.editDeadline.description;
    }
    if (this.editDeadline.priority) {
      dto.priority = this.editDeadline.priority;
    }
    dto.isCompleted = this.editDeadline.isCompleted;
    if (this.editDeadline.notes !== '') {
      dto.notes = this.editDeadline.notes;
    }

    this.caseService.updateDeadline(this.caseId, this.editingDeadlineId, dto).subscribe({
      next: updated => {
        if (this.case) {
          this.case.deadlines = this.case.deadlines.map(d =>
            d.deadlineId === updated.deadlineId ? updated : d
          );
        }
        this.cancelEditDeadline();
      },
      error: error => {
        console.error('Error updating deadline:', error);
      }
    });
  }

  private toDateInputValue(dateStr: string): string {
    const d = new Date(dateStr);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toTimeInputValue(timeStr: string): string {
    if (!timeStr) {
      return '';
    }
    // Expecting formats like HH:mm or HH:mm:ss, but we only need HH:mm for input[type=time]
    return timeStr.length >= 5 ? timeStr.substring(0, 5) : timeStr;
  }

  loadCase(id: string): void {
    this.isLoading = true;
    this.caseService.getCaseDetail(id).subscribe({
      next: (caseData) => {
        this.case = caseData;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading case:', error);
        this.isLoading = false;
      }
    });
  }

  editCase(): void {
    if (this.caseId) {
      this.router.navigate(['/case-management/edit', this.caseId]);
    }
  }

  deleteCase(): void {
    if (this.caseId && confirm('Are you sure you want to delete this case?')) {
      this.caseService.deleteCase(this.caseId).subscribe({
        next: () => this.router.navigate(['/case-management']),
        error: (error) => {
          console.error('Error deleting case:', error);
          alert('Failed to delete case');
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/case-management']);
  }

  getPriorityClass(priority?: string): string {
    const normalized = this.normalizePriority(priority);
    if (!normalized) return '';
    const classes: Record<CasePriority, string> = {
      [CasePriority.LOW]: 'priority-low',
      [CasePriority.MEDIUM]: 'priority-medium',
      [CasePriority.HIGH]: 'priority-high',
      [CasePriority.CRITICAL]: 'priority-critical'
    };
    return classes[normalized] || '';
  }

  getStatusClass(status?: string): string {
    const normalized = this.normalizeStatus(status);
    if (!normalized) return '';
    const classes: Record<CaseStatus, string> = {
      [CaseStatus.OPEN]: 'status-open',
      [CaseStatus.IN_PROGRESS]: 'status-in-progress',
      [CaseStatus.PENDING]: 'status-pending',
      [CaseStatus.RESOLVED]: 'status-resolved',
      [CaseStatus.CLOSED]: 'status-closed'
    };
    return classes[normalized] || '';
  }

  isDueSoon(): boolean {
    if (!this.case?.endDate) return false;
    const dueDate = new Date(this.case.endDate);
    const today = new Date();
    const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    return diffDays <= 3 && diffDays >= 0;
  }

  isOverdue(): boolean {
    if (!this.case?.endDate) return false;
    return new Date(this.case.endDate) < new Date();
  }

  private normalizeStatus(status?: string): CaseStatus | undefined {
    if (!status) {
      return undefined;
    }
    const normalized = status.toLowerCase();
    if (normalized.includes('pending')) return CaseStatus.PENDING;
    if (normalized.includes('progress') || normalized.includes('active')) return CaseStatus.IN_PROGRESS;
    if (normalized.includes('closed') || normalized.includes('complete')) return CaseStatus.CLOSED;
    if (normalized.includes('resolved')) return CaseStatus.RESOLVED;
    return CaseStatus.OPEN;
  }

  private normalizePriority(priority?: string): CasePriority | undefined {
    if (!priority) {
      return undefined;
    }
    const normalized = priority.toLowerCase();
    if (normalized.includes('low')) return CasePriority.LOW;
    if (normalized.includes('high') && !normalized.includes('critical')) return CasePriority.HIGH;
    if (normalized.includes('critical')) return CasePriority.CRITICAL;
    return CasePriority.MEDIUM;
  }

  createHearing(): void {
    if (!this.caseId || !this.newHearing.date || !this.newHearing.time) {
      return;
    }

    const dto: CreateHearingApiDto = {
      date: new Date(this.newHearing.date).toISOString(),
      time: this.formatTime(this.newHearing.time),
      courtId: this.case?.court?.courtId ?? null,
      location: this.newHearing.location || null,
      hearingType: this.newHearing.hearingType || null,
      remarks: this.newHearing.remarks || null
    };

    this.caseService.addHearing(this.caseId, dto).subscribe({
      next: hearing => {
        if (this.case) {
          this.case.hearings = [...this.case.hearings, hearing];
        }
        this.newHearing = {
          date: '',
          time: '',
          location: '',
          hearingType: '',
          remarks: ''
        };
      },
      error: error => {
        console.error('Error creating hearing:', error);
      }
    });
  }

  createDeadline(): void {
    if (!this.caseId || !this.newDeadline.dueDate || !this.newDeadline.description) {
      return;
    }

    const dto: CreateDeadlineApiDto = {
      dueDate: new Date(this.newDeadline.dueDate).toISOString(),
      description: this.newDeadline.description,
      priority: this.newDeadline.priority || 'Medium',
      notes: this.newDeadline.notes || null
    };

    this.caseService.addDeadline(this.caseId, dto).subscribe({
      next: deadline => {
        if (this.case) {
          this.case.deadlines = [...this.case.deadlines, deadline];
        }
        this.newDeadline = {
          dueDate: '',
          description: '',
          priority: 'Medium',
          notes: ''
        };
      },
      error: error => {
        console.error('Error creating deadline:', error);
      }
    });
  }

  private formatTime(time: string): string {
    if (!time) {
      return '00:00:00';
    }
    return time.length === 5 ? `${time}:00` : time;
  }
}

