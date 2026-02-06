import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CaseService } from '../../services/case.service';
import { Case, CaseStatus, CasePriority } from '../../models/case.model';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { LawyerDto, CourtDto, PartyDto, CreateCaseApiDto, CasePartyDto } from '../../models/case-api.model';

@Component({
  standalone: true,
  selector: 'app-case-form',
  templateUrl: './case-form.component.html',
  styleUrls: ['./case-form.component.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class CaseFormComponent implements OnInit {
  caseForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  caseId?: string;
  lawyers: LawyerDto[] = [];
  courts: CourtDto[] = [];
  parties: PartyDto[] = [];
  
  caseStatuses = Object.values(CaseStatus);
  casePriorities = Object.values(CasePriority);

  constructor(
    private fb: FormBuilder,
    private caseService: CaseService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadLookups();
    
    this.route.paramMap.subscribe(params => {
      this.caseId = params.get('id') || undefined;
      this.isEditMode = !!this.caseId;
      
      if (this.isEditMode && this.caseId) {
        this.loadCase(this.caseId);
      }
    });
  }

  private loadLookups(): void {
    const baseUrl = environment.apiBaseUrl;

    this.http.get<LawyerDto[]>(`${baseUrl}/api/lawyers`).subscribe({
      next: data => (this.lawyers = data),
      error: error => console.error('Error loading lawyers', error)
    });

    this.http.get<CourtDto[]>(`${baseUrl}/api/courts`).subscribe({
      next: data => (this.courts = data),
      error: error => console.error('Error loading courts', error)
    });

    this.http.get<PartyDto[]>(`${baseUrl}/api/parties`).subscribe({
      next: data => (this.parties = data),
      error: error => console.error('Error loading parties', error)
    });
  }

  initForm(): void {
    this.caseForm = this.fb.group({
      caseNumber: ['', [Validators.maxLength(50)]],
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      status: [CaseStatus.OPEN, Validators.required],
      priority: [CasePriority.MEDIUM, Validators.required],
      assignedTo: [''],
      dueDate: [''],
      tags: [''],
      notes: [''],
      dateFiled: ['', Validators.required],
      assignedLawyerId: ['', Validators.required],
      courtId: ['', Validators.required],
      plaintiffPartyId: ['', Validators.required],
      defendantPartyId: ['', Validators.required],
      additionalLawyerIds: [[]]
    });
  }

  loadCase(id: string): void {
    this.isLoading = true;
    this.caseService.getCaseById(id).subscribe({
      next: (caseData) => {
        if (caseData) {
          this.caseForm.patchValue({
            caseNumber: '',
            title: caseData.title,
            description: caseData.description,
            status: caseData.status,
            priority: caseData.priority,
            assignedTo: caseData.assignedTo || '',
            dueDate: caseData.dueDate ? this.formatDate(caseData.dueDate) : '',
            tags: caseData.tags?.join(', ') || '',
            notes: caseData.notes || ''
          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading case:', error);
        this.isLoading = false;
      }
    });
  }

  formatDate(date: Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  onSubmit(): void {
    if (this.caseForm.invalid) {
      this.markFormGroupTouched(this.caseForm);
      return;
    }

    this.isLoading = true;
    const formValue = this.caseForm.value;
    const caseNumber = (formValue.caseNumber as string | undefined)?.trim() || this.generateCaseNumber();
    
    const caseData = {
      title: formValue.title,
      description: formValue.description,
      status: formValue.status,
      priority: formValue.priority,
      assignedTo: formValue.assignedTo || undefined,
      dueDate: formValue.dueDate ? new Date(formValue.dueDate) : undefined,
      tags: formValue.tags ? formValue.tags.split(',').map((t: string) => t.trim()) : [],
      notes: formValue.notes || undefined
    };

    if (this.isEditMode && this.caseId) {
      this.caseService.updateCase({ id: this.caseId, ...caseData }).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/case-management']);
        },
        error: (error) => {
          console.error('Error updating case:', error);
          this.isLoading = false;
        }
      });
    } else {
      const rawAdditional = (formValue.additionalLawyerIds as (string | number)[] | undefined) || [];
      const additionalLawyerIds = rawAdditional
        .map(v => Number(v))
        .filter(v => !Number.isNaN(v) && v !== Number(formValue.assignedLawyerId));

      const createDto: CreateCaseApiDto = {
        caseNumber,
        title: formValue.title,
        description: formValue.description,
        assignedLawyerId: Number(formValue.assignedLawyerId),
        plaintiffLawyerId: null,
        defendantLawyerId: null,
        courtId: Number(formValue.courtId),
        dateFiled: new Date(formValue.dateFiled).toISOString(),
        startDate: null,
        endDate: caseData.dueDate ? caseData.dueDate.toISOString() : null,
        status: formValue.status,
        priority: formValue.priority,
        parties: [
          {
            partyId: Number(formValue.plaintiffPartyId),
            role: 'Plaintiff',
            side: 'Accuser'
          } as CasePartyDto,
          {
            partyId: Number(formValue.defendantPartyId),
            role: 'Defendant',
            side: 'Accused'
          } as CasePartyDto
        ],
        additionalLawyerIds
      };

      this.caseService.createCase(createDto).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/case-management']);
        },
        error: (error) => {
          console.error('Error creating case:', error);
          const msg = this.extractServerError(error);
          alert(`Failed to create case: ${msg}`);
          this.isLoading = false;
        }
      });
    }
  }

  private generateCaseNumber(): string {
    const date = new Date();
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    const hh = String(date.getHours()).padStart(2, '0');
    const mm = String(date.getMinutes()).padStart(2, '0');
    const ss = String(date.getSeconds()).padStart(2, '0');
    const ms = String(date.getMilliseconds()).padStart(3, '0');
    const rand = Math.random().toString(36).slice(2, 5).toUpperCase();
    return `CASE-${y}${m}${d}-${hh}${mm}${ss}${ms}-${rand}`;
  }

  onCancel(): void {
    this.router.navigate(['/case-management']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.caseForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.caseForm.get(fieldName);
    if (field?.hasError('required')) {
      return 'This field is required';
    }
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `Minimum length is ${minLength} characters`;
    }
    return '';
  }

  private extractServerError(error: any): string {
    const err = error?.error ?? error;
    if (!err) return 'Unknown error';
    if (typeof err === 'string') return err;
    if (err?.message) return err.message;
    if (err?.title) return err.title;
    try {
      return JSON.stringify(err);
    } catch {
      return 'Unknown error';
    }
  }
}