import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  WorkflowTemplate, 
  CaseWorkflow, 
  CreateWorkflowTemplateDto, 
  StartWorkflowDto, 
  ApproveStepDto, 
  AssignStepDto,
  WorkflowTask 
} from '../models/workflow.model';

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/workflows`;

  constructor(private http: HttpClient) {}

  // Workflow Templates
  getWorkflowTemplates(): Observable<WorkflowTemplate[]> {
    return this.http.get<WorkflowTemplate[]>(`${this.apiUrl}/templates`);
  }

  createWorkflowTemplate(template: CreateWorkflowTemplateDto): Observable<WorkflowTemplate> {
    return this.http.post<WorkflowTemplate>(`${this.apiUrl}/templates`, template);
  }

  // Case Workflows
  startCaseWorkflow(caseId: number, startDto: StartWorkflowDto): Observable<CaseWorkflow> {
    return this.http.post<CaseWorkflow>(`${this.apiUrl}/cases/${caseId}/start`, startDto);
  }

  getCaseWorkflow(caseId: number): Observable<CaseWorkflow> {
    return this.http.get<CaseWorkflow>(`${this.apiUrl}/cases/${caseId}`);
  }

  // Workflow Steps
  approveWorkflowStep(stepId: number, approveDto: ApproveStepDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/steps/${stepId}/approve`, approveDto);
  }

  assignStepToUser(stepId: number, assignDto: AssignStepDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/steps/${stepId}/assign`, assignDto);
  }

  // Tasks
  getPendingTasks(userId: number): Observable<WorkflowTask[]> {
    return this.http.get<WorkflowTask[]>(`${this.apiUrl}/tasks/pending/${userId}`);
  }
}
