export enum CaseStatus {
  OPEN = 'OPEN',
  IN_PROGRESS = 'IN_PROGRESS',
  PENDING = 'PENDING',
  RESOLVED = 'RESOLVED',
  CLOSED = 'CLOSED'
}

export enum CasePriority {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

export interface Case {
  id: string;
  caseNumber: string;
  title: string;
  description: string;
  status: CaseStatus;
  priority: CasePriority;
  assignedTo?: string;
  lawyerName?: string;
  courtName?: string;
  dateFiled: Date;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
  dueDate?: Date;
  tags?: string[];
  notes?: string;
}

export interface CreateCaseDto {
  title: string;
  description: string;
  status: CaseStatus;
  priority: CasePriority;
  assignedTo?: string;
  dueDate?: Date;
  tags?: string[];
  notes?: string;
}

export interface UpdateCaseDto extends Partial<CreateCaseDto> {
  id: string;
}