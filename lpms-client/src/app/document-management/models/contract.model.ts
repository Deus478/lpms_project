export interface Contract {
  contractId: number;
  contractNumber: string;
  title: string;
  description?: string;
  contractType: string;
  status: string;
  counterparty?: string;
  contractValue: number;
  currency: string;
  startDate: Date;
  endDate?: Date;
  executionDate?: Date;
  autoRenew: boolean;
  renewalNoticeDays?: number;
  riskLevel: string;
  createdAt: Date;
  updatedAt?: Date;
  documents: ContractDocument[];
  approvals: ContractApproval[];
}

export interface ContractDocument {
  contractDocumentId: number;
  contractId: number;
  documentId: string;
  documentType: string;
  version: number;
  uploadedAt: Date;
  description?: string;
}

export interface ContractApproval {
  contractApprovalId: number;
  contractId: number;
  approverUserId: number;
  approvalLevel: string;
  status: string;
  comments?: string;
  requestedAt: Date;
  reviewedAt?: Date;
  approverUser?: User;
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

export interface CreateContractDto {
  title: string;
  description?: string;
  contractType: string;
  requestingDepartmentId: number;
  counterparty?: string;
  contractValue: number;
  currency: string;
  startDate: Date;
  endDate?: Date;
  autoRenew: boolean;
  renewalNoticeDays?: number;
  riskLevel: string;
}

export interface UpdateContractStatusDto {
  status: string;
}

export interface AddContractDocumentDto {
  documentId: string;
  documentType: string;
  description?: string;
}

export interface SubmitForApprovalDto {
  approverUserId: number;
  approvalLevel: string;
  comments?: string;
}
