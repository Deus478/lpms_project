import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

import { CaseFormComponent } from './case-form.component';
import { CaseService } from '../../services/case.service';
import { CaseStatus, CasePriority } from '../../models/case.model';

describe('CaseFormComponent', () => {
  let component: CaseFormComponent;
  let fixture: ComponentFixture<CaseFormComponent>;
  let caseService: jasmine.SpyObj<CaseService>;

  const mockCase = {
    id: '1',
    title: 'Test Case',
    description: 'Test Description',
    status: CaseStatus.OPEN,
    priority: CasePriority.HIGH,
    createdBy: 'User',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(async () => {
    const caseServiceSpy = jasmine.createSpyObj('CaseService', ['getCaseById', 'createCase', 'updateCase']);

    await TestBed.configureTestingModule({
      declarations: [ CaseFormComponent ],
      imports: [ ReactiveFormsModule, RouterTestingModule ],
      providers: [
        { provide: CaseService, useValue: caseServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of({ get: () => null })
          }
        }
      ]
    })
    .compileComponents();

    caseService = TestBed.inject(CaseService) as jasmine.SpyObj<CaseService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CaseFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values', () => {
    expect(component.caseForm.value.status).toBe(CaseStatus.OPEN);
    expect(component.caseForm.value.priority).toBe(CasePriority.MEDIUM);
  });

  it('should validate required fields', () => {
    const titleControl = component.caseForm.get('title');
    const descriptionControl = component.caseForm.get('description');

    expect(titleControl?.valid).toBeFalsy();
    expect(descriptionControl?.valid).toBeFalsy();

    titleControl?.setValue('Test Title');
    descriptionControl?.setValue('Test Description Long Enough');

    expect(titleControl?.valid).toBeTruthy();
    expect(descriptionControl?.valid).toBeTruthy();
  });

  it('should not submit invalid form', () => {
    component.onSubmit();
    expect(caseService.createCase).not.toHaveBeenCalled();
  });

  it('should create case with valid form', () => {
    caseService.createCase.and.returnValue(of(mockCase));

    component.caseForm.patchValue({
      title: 'New Case',
      description: 'New Description',
      status: CaseStatus.OPEN,
      priority: CasePriority.MEDIUM
    });

    component.onSubmit();
    expect(caseService.createCase).toHaveBeenCalled();
  });

  it('should load case in edit mode', () => {
    component.isEditMode = true;
    component.caseId = '1';
    caseService.getCaseById.and.returnValue(of(mockCase));

    component.loadCase('1');

    expect(caseService.getCaseById).toHaveBeenCalledWith('1');
  });
});