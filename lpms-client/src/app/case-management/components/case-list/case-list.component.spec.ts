import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { CaseListComponent } from './case-list.component';
import { CaseService } from '../../services/case.service';
import { CaseStatus, CasePriority } from '../../models/case.model';

describe('CaseListComponent', () => {
  let component: CaseListComponent;
  let fixture: ComponentFixture<CaseListComponent>;
  let caseService: jasmine.SpyObj<CaseService>;

  const mockCases = [
    {
      id: '1',
      title: 'Test Case 1',
      description: 'Description 1',
      status: CaseStatus.OPEN,
      priority: CasePriority.HIGH,
      createdBy: 'User',
      createdAt: new Date(),
      updatedAt: new Date()
    },
    {
      id: '2',
      title: 'Test Case 2',
      description: 'Description 2',
      status: CaseStatus.IN_PROGRESS,
      priority: CasePriority.MEDIUM,
      createdBy: 'User',
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  beforeEach(async () => {
    const caseServiceSpy = jasmine.createSpyObj('CaseService', ['getCases', 'deleteCase']);

    await TestBed.configureTestingModule({
      declarations: [ CaseListComponent ],
      imports: [ RouterTestingModule, FormsModule ],
      providers: [
        { provide: CaseService, useValue: caseServiceSpy }
      ]
    })
    .compileComponents();

    caseService = TestBed.inject(CaseService) as jasmine.SpyObj<CaseService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CaseListComponent);
    component = fixture.componentInstance;
    caseService.getCases.and.returnValue(of(mockCases));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load cases on init', () => {
    expect(caseService.getCases).toHaveBeenCalled();
    expect(component.cases.length).toBe(2);
    expect(component.filteredCases.length).toBe(2);
  });

  it('should filter cases by search term', () => {
    component.searchTerm = 'Test Case 1';
    component.onSearch();
    expect(component.filteredCases.length).toBe(1);
    expect(component.filteredCases[0].id).toBe('1');
  });

  it('should filter cases by status', () => {
    component.selectedStatus = CaseStatus.OPEN;
    component.onStatusFilterChange();
    expect(component.filteredCases.length).toBe(1);
    expect(component.filteredCases[0].status).toBe(CaseStatus.OPEN);
  });

  it('should filter cases by priority', () => {
    component.selectedPriority = CasePriority.HIGH;
    component.onPriorityFilterChange();
    expect(component.filteredCases.length).toBe(1);
    expect(component.filteredCases[0].priority).toBe(CasePriority.HIGH);
  });

  it('should delete case', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    caseService.deleteCase.and.returnValue(of(true));
    
    component.deleteCase('1');
    
    expect(caseService.deleteCase).toHaveBeenCalledWith('1');
    expect(caseService.getCases).toHaveBeenCalled();
  });
});