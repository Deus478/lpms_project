import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

import { CaseDetailsComponent } from './case-details.component';
import { CaseService } from '../../services/case.service';
import { CaseStatus, CasePriority } from '../../models/case.model';

describe('CaseDetailsComponent', () => {
  let component: CaseDetailsComponent;
  let fixture: ComponentFixture<CaseDetailsComponent>;
  let caseService: jasmine.SpyObj<CaseService>;

  const mockCase = {
    id: '1',
    title: 'Test Case',
    description: 'Test Description',
    status: CaseStatus.OPEN,
    priority: CasePriority.HIGH,
    assignedTo: 'John Doe',
    createdBy: 'User',
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-01-02'),
    dueDate: new Date('2024-12-31'),
    tags: ['test', 'example']
  };

  beforeEach(async () => {
    const caseServiceSpy = jasmine.createSpyObj('CaseService', ['getCaseById', 'deleteCase']);

    await TestBed.configureTestingModule({
      declarations: [ CaseDetailsComponent ],
      imports: [ RouterTestingModule ],
      providers: [
        { provide: CaseService, useValue: caseServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of({ get: () => '1' })
          }
        }
      ]
    })
    .compileComponents();

    caseService = TestBed.inject(CaseService) as jasmine.SpyObj<CaseService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CaseDetailsComponent);
    component = fixture.componentInstance;
    caseService.getCaseById.and.returnValue(of(mockCase));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load case on init', () => {
    expect(caseService.getCaseById).toHaveBeenCalledWith('1');
    expect(component.case).toEqual(mockCase);
  });

  it('should check if case is due soon', () => {
    const threeDaysFromNow = new Date();
    threeDaysFromNow.setDate(threeDaysFromNow.getDate() + 2);
    
    if (component.case) {
      component.case.dueDate = threeDaysFromNow;
    }
    
    expect(component.isDueSoon()).toBeTruthy();
  });

  it('should check if case is overdue', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    
    if (component.case) {
      component.case.dueDate = yesterday;
    }
    
    expect(component.isOverdue()).toBeTruthy();
  });

  it('should delete case', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    caseService.deleteCase.and.returnValue(of(true));
    
    component.caseId = '1';
    component.deleteCase();
    
    expect(caseService.deleteCase).toHaveBeenCalledWith('1');
  });

  it('should return correct priority class', () => {
    expect(component.getPriorityClass(CasePriority.HIGH)).toBe('priority-high');
    expect(component.getPriorityClass(CasePriority.LOW)).toBe('priority-low');
  });

  it('should return correct status class', () => {
    expect(component.getStatusClass(CaseStatus.OPEN)).toBe('status-open');
    expect(component.getStatusClass(CaseStatus.CLOSED)).toBe('status-closed');
  });
});