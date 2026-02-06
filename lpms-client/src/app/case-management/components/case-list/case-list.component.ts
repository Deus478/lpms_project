import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Case, CaseStatus, CasePriority } from '../../models/case.model';
import { CaseService } from '../../services/case.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
    standalone:true,
  selector: 'app-case-list',
  template: `
    <div class="case-list-container">
      <div class="header">
        <h1>Case Management</h1>
        <div class="header-actions">
          <button class="btn btn-primary" (click)="createNewCase()">Create Case</button>
        </div>
      </div>

      <div class="filters">
        <div class="search-box">
          <input
            type="text"
            [(ngModel)]="searchTerm"
            (input)="onSearch()"
            placeholder="Search by title or description"
            class="search-input"
          />
        </div>

        <div class="filter-group">
          <label for="statusFilter">Status:</label>
          <select
            id="statusFilter"
            [(ngModel)]="selectedStatus"
            (change)="onStatusFilterChange()"
            class="filter-select"
          >
            <option value="ALL">All Statuses</option>
            <option *ngFor="let s of caseStatuses" [value]="s">{{ s.replace('_',' ') }}</option>
          </select>
        </div>

        <div class="filter-group">
          <label for="priorityFilter">Priority:</label>
          <select
            id="priorityFilter"
            [(ngModel)]="selectedPriority"
            (change)="onPriorityFilterChange()"
            class="filter-select"
          >
            <option value="ALL">All Priorities</option>
            <option *ngFor="let p of casePriorities" [value]="p">{{ p }}</option>
          </select>
        </div>
      </div>

      <div class="loading" *ngIf="isLoading">
        Loading cases...
      </div>

      <div class="cases-table" *ngIf="!isLoading && pagedCases.length > 0">
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Case #</th>
              <th>Lawyer</th>
              <th>Court</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Filed</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let c of pagedCases">
              <td>
                <button class="link-button" (click)="viewCase(c.id)">{{ c.title || ('Case ' + c.caseNumber) }}</button>
              </td>
              <td>{{ c.caseNumber }}</td>
              <td>{{ c.lawyerName || '-' }}</td>
              <td>{{ c.courtName || '-' }}</td>
              <td>
                <span class="badge" [ngClass]="getStatusClass(c.status)">{{ c.status.replace('_',' ') }}</span>
              </td>
              <td>
                <span class="badge" [ngClass]="getPriorityClass(c.priority)">{{ c.priority }}</span>
              </td>
              <td>{{ c.dateFiled | date:'shortDate' }}</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-small" (click)="editCase(c.id)">Edit</button>
                  <button class="btn-small danger" (click)="deleteCase(c.id)">Delete</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="pagination">
          <button class="btn-small" (click)="goToPage(currentPage - 1)" [disabled]="currentPage === 1">Prev</button>
          <span>Page {{ currentPage }} of {{ totalPages }}</span>
          <button class="btn-small" (click)="goToPage(currentPage + 1)" [disabled]="currentPage === totalPages">Next</button>
        </div>
      </div>

      <div class="no-results" *ngIf="!isLoading && pagedCases.length === 0">
        <p>No cases found.</p>
      </div>
    </div>
  `,
  styleUrls: ['./case-list.component.css'],
  imports:[CommonModule, FormsModule]
})
export class CaseListComponent implements OnInit {
  cases: Case[] = [];
  filteredCases: Case[] = [];
  pagedCases: Case[] = [];
  isLoading = false;
  searchTerm = '';
  selectedStatus: CaseStatus | 'ALL' = 'ALL';
  selectedPriority: CasePriority | 'ALL' = 'ALL';

  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  caseStatuses = Object.values(CaseStatus);
  casePriorities = Object.values(CasePriority);

  constructor(
    private caseService: CaseService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCases();
  }

  loadCases(): void {
    this.isLoading = true;
    this.caseService.getCases().subscribe({
      next: (cases) => {
        this.cases = cases;
        this.filteredCases = cases;
        this.updatePagination();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading cases:', error);
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  onStatusFilterChange(): void {
    this.applyFilters();
  }

  onPriorityFilterChange(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    this.filteredCases = this.cases.filter(caseItem => {
      const matchesSearch = !this.searchTerm || 
        caseItem.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        caseItem.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesStatus = this.selectedStatus === 'ALL' || 
        caseItem.status === this.selectedStatus;

      const matchesPriority = this.selectedPriority === 'ALL' || 
        caseItem.priority === this.selectedPriority;

      return matchesSearch && matchesStatus && matchesPriority;
    });

    this.currentPage = 1;
    this.updatePagination();
  }

  updatePagination(): void {
    this.totalPages = Math.max(1, Math.ceil(this.filteredCases.length / this.pageSize));
    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedCases = this.filteredCases.slice(start, end);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.updatePagination();
  }

  viewCase(caseId: string): void {
    this.router.navigate(['/case-management/details', caseId]);
  }

  editCase(caseId: string): void {
    this.router.navigate(['/case-management/edit', caseId]);
  }

  deleteCase(caseId: string): void {
    if (confirm('Are you sure you want to delete this case?')) {
      this.caseService.deleteCase(caseId).subscribe({
        next: () => {
          this.loadCases();
        },
        error: (error) => {
          console.error('Error deleting case:', error);
          alert('Failed to delete case');
        }
      });
    }
  }

  createNewCase(): void {
    this.router.navigate(['/case-management/create']);
  }

  getPriorityClass(priority: CasePriority): string {
    const classes: { [key in CasePriority]: string } = {
      [CasePriority.LOW]: 'priority-low',
      [CasePriority.MEDIUM]: 'priority-medium',
      [CasePriority.HIGH]: 'priority-high',
      [CasePriority.CRITICAL]: 'priority-critical'
    };
    return classes[priority];
  }

  getStatusClass(status: CaseStatus): string {
    const classes: { [key in CaseStatus]: string } = {
      [CaseStatus.OPEN]: 'status-open',
      [CaseStatus.IN_PROGRESS]: 'status-in-progress',
      [CaseStatus.PENDING]: 'status-pending',
      [CaseStatus.RESOLVED]: 'status-resolved',
      [CaseStatus.CLOSED]: 'status-closed'
    };
    return classes[status];
  }
}