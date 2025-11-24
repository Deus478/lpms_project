export interface CaseSummaryDto {
  caseId: number;
  caseNumber: string;
  title: string;
  lawyerName: string;
  status: string;
  priority: string;
  courtName: string;
  dateFiled: string;
}

export interface LawyerDto {
  lawyerId: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string | null;
  barNumber?: string | null;
  specialization?: string | null;
}

export interface CourtDto {
  courtId: number;
  name: string;
  type?: string | null;
  level?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  zipCode?: string | null;
  phone?: string | null;
}

export interface CasePartyDto {
  partyId: number;
  role: string;
  side: string;
}

export interface PartyDto {
  partyId: number;
  firstName: string;
  lastName: string;
  fullName: string;
  partyType?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  zipCode?: string | null;
}

export interface CasePartyDetailDto {
  casePartyId: number;
  role: string;
  party: PartyDto;
}

// DTO used when creating a case via the API
export interface CreateCaseApiDto {
  caseNumber: string;
  title: string;
  description?: string | null;
  assignedLawyerId: number;
  plaintiffLawyerId?: number | null;
  defendantLawyerId?: number | null;
  courtId: number;
  dateFiled: string; // ISO string
  startDate?: string | null;
  endDate?: string | null;
  status?: string | null;
  priority?: string | null;
  parties: CasePartyDto[];
  additionalLawyerIds?: number[] | null;
}

export interface HearingDto {
  hearingId: number;
  caseId: number;
  courtId?: number | null;
  date: string;
  time: string;
  location?: string | null;
  hearingType?: string | null;
  remarks?: string | null;
  status: string;
  createdAt: string;
  court?: CourtDto | null;
}

export interface DeadlineDto {
  deadlineId: number;
  caseId: number;
  dueDate: string;
  description: string;
  priority: string;
  isCompleted: boolean;
  completedDate?: string | null;
  notes?: string | null;
  createdAt: string;
}

export interface CreateHearingApiDto {
  date: string;
  time: string;
  courtId?: number | null;
  location?: string | null;
  hearingType?: string | null;
  remarks?: string | null;
}

export interface UpdateHearingApiDto {
  date?: string | null;
  time?: string | null;
  courtId?: number | null;
  location?: string | null;
  hearingType?: string | null;
  remarks?: string | null;
  status?: string | null;
}

export interface CreateDeadlineApiDto {
  dueDate: string;
  description: string;
  priority?: string | null;
  notes?: string | null;
}

export interface UpdateDeadlineApiDto {
  dueDate?: string | null;
  description?: string | null;
  priority?: string | null;
  isCompleted?: boolean | null;
  notes?: string | null;
}

export interface CaseDetailDto {
  caseId: number;
  caseNumber: string;
  title: string;
  description?: string | null;
  status: string;
  priority: string;
  outcome?: string | null;
  dateFiled: string;
  startDate?: string | null;
  endDate?: string | null;
  createdAt: string;
  updatedAt?: string | null;

  assignedLawyer: LawyerDto;
  plaintiffLawyer?: LawyerDto | null;
  defendantLawyer?: LawyerDto | null;
  court: CourtDto;
  parties: CasePartyDetailDto[];
  hearings: HearingDto[];
  deadlines: DeadlineDto[];
  additionalLawyers: LawyerDto[];
}
