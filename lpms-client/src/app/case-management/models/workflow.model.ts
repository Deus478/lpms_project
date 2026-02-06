export interface WorkflowTemplate {
  workflowTemplateId: number;
  name: string;
  description?: string;
  caseType: string;
  isActive: boolean;
  createdAt: Date;
  stepTemplates: WorkflowStepTemplate[];
}

export interface WorkflowStepTemplate {
  workflowStepTemplateId: number;
  workflowTemplateId: number;
  stepName: string;
  description?: string;
  stepOrder: number;
  approverRole: string;
  action: string;
  timeoutHours?: number;
  isOptional: boolean;
}

export interface CaseWorkflow {
  caseWorkflowId: number;
  caseId: number;
  workflowTemplateId: number;
  status: string;
  startedAt: Date;
  completedAt?: Date;
  workflowTemplate: WorkflowTemplate;
  workflowSteps: WorkflowStep[];
}

export interface WorkflowStep {
  caseWorkflowStepId: number;
  caseWorkflowId: number;
  stepName: string;
  stepOrder: number;
  approverRole: string;
  status: string;
  assignedUserId?: number;
  startedAt?: Date;
  completedAt?: Date;
  dueDate?: Date;
  comments?: string;
  legalOpinion?: string;
  assignedUser?: User;
}

export interface User {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  title?: string;
  department?: string;
  employeeId?: string;
  phoneNumber?: string;
  isActive: boolean;
  createdAt: Date;
  lastLoginAt?: Date;
  userRoles: UserRole[];
  fullName: string;
}

export interface UserRole {
  userRoleId: number;
  userId: number;
  roleId: number;
  assignedAt: Date;
  expiresAt?: Date;
  isActive: boolean;
  role?: Role;
}

export interface Role {
  roleId: number;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  permissions: Permission[];
}

export interface Permission {
  permissionId: number;
  name: string;
  description?: string;
  module: string;
  action: string;
  createdAt: Date;
}

export interface CreateWorkflowTemplateDto {
  name: string;
  description?: string;
  caseType: string;
  steps: CreateWorkflowStepTemplateDto[];
}

export interface CreateWorkflowStepTemplateDto {
  stepName: string;
  description?: string;
  stepOrder: number;
  approverRole: string;
  action: string;
  timeoutHours?: number;
  isOptional: boolean;
}

export interface StartWorkflowDto {
  workflowTemplateId: number;
}

export interface ApproveStepDto {
  action: string;
  comments?: string;
  legalOpinion?: string;
}

export interface AssignStepDto {
  userId: number;
}

export interface WorkflowTask {
  caseWorkflowStepId: number;
  stepName: string;
  caseNumber: string;
  caseTitle: string;
  dueDate?: Date;
  status: string;
  caseId: number;
}
