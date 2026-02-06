import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CaseService } from '../../services/case.service';
import { WorkflowStep } from '../../models/workflow.model';

@Component({
  selector: 'app-workflow-tracker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workflow-tracker.component.html',
  styleUrls: ['./workflow-tracker.component.css']
})
export class WorkflowTrackerComponent implements OnInit {
  @Input() caseId!: number;
  workflowSteps: WorkflowStep[] = [];
  loading = false;
  error: string | null = null;

  constructor(private caseService: CaseService) {}

  ngOnInit(): void {
    if (this.caseId) {
      this.loadWorkflow();
    }
  }

  loadWorkflow(): void {
    this.loading = true;
    this.error = null;
    
    this.caseService.getCaseWorkflow(this.caseId).subscribe({
      next: (workflow) => {
        this.workflowSteps = workflow.workflowSteps || [];
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load workflow information';
        this.loading = false;
        console.error('Workflow loading error:', err);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'in progress':
        return 'bg-blue-100 text-blue-800';
      case 'pending':
        return 'bg-gray-100 text-gray-800';
      case 'rejected':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed':
        return '✓';
      case 'in progress':
        return '⏳';
      case 'pending':
        return '⏸';
      case 'rejected':
        return '✗';
      default:
        return '?';
    }
  }

  isStepActive(step: WorkflowStep, index: number): boolean {
    const currentIndex = this.workflowSteps.findIndex(s => s.status === 'In Progress');
    return index === currentIndex;
  }

  isStepCompleted(step: WorkflowStep): boolean {
    return step.status === 'Completed' || step.status === 'Approved';
  }

  isStepRejected(step: WorkflowStep): boolean {
    return step.status === 'Rejected';
  }

  isStepOverdue(step: WorkflowStep): boolean {
    if (!step.dueDate || step.status !== 'Pending') {
      return false;
    }
    return new Date(step.dueDate) < new Date();
  }
}
