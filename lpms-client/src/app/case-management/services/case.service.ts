import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { Case, CreateCaseDto, UpdateCaseDto, CaseStatus, CasePriority } from '../models/case.model';
import { CaseSummaryDto, CaseDetailDto, CreateCaseApiDto, HearingDto, DeadlineDto, CreateHearingApiDto, CreateDeadlineApiDto, UpdateHearingApiDto, UpdateDeadlineApiDto } from '../models/case-api.model';
import { CaseWorkflow } from '../models/workflow.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CaseService {
  private cases: Case[] = [];

  private casesSubject = new BehaviorSubject<Case[]>(this.cases);
  public cases$ = this.casesSubject.asObservable();
  private readonly baseUrl = `${environment.apiBaseUrl}/api/cases`;

  constructor(private http: HttpClient) {}

  getCases(): Observable<Case[]> {
    return this.http.get<CaseSummaryDto[]>(this.baseUrl).pipe(
      map(dtos => dtos.map(dto => this.mapSummaryDtoToCase(dto))),
      tap(cases => {
        this.cases = cases;
        this.casesSubject.next(this.cases);
      }),
      catchError(err => throwError(() => err))
    );
  }

  updateHearing(caseId: string, hearingId: number, dto: UpdateHearingApiDto): Observable<HearingDto> {
    const numericCaseId = Number(caseId);
    if (Number.isNaN(numericCaseId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.put<HearingDto>(`${this.baseUrl}/${numericCaseId}/hearings/${hearingId}`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  addHearing(caseId: string, dto: CreateHearingApiDto): Observable<HearingDto> {
    const numericId = Number(caseId);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.post<HearingDto>(`${this.baseUrl}/${numericId}/hearings`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  addDeadline(caseId: string, dto: CreateDeadlineApiDto): Observable<DeadlineDto> {
    const numericId = Number(caseId);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.post<DeadlineDto>(`${this.baseUrl}/${numericId}/deadlines`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  updateDeadline(caseId: string, deadlineId: number, dto: UpdateDeadlineApiDto): Observable<DeadlineDto> {
    const numericCaseId = Number(caseId);
    if (Number.isNaN(numericCaseId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.put<DeadlineDto>(`${this.baseUrl}/${numericCaseId}/deadlines/${deadlineId}`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getCaseDetail(id: string): Observable<CaseDetailDto> {
    const numericId = Number(id);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.get<CaseDetailDto>(`${this.baseUrl}/${numericId}`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getCaseById(id: string): Observable<Case | undefined> {
    const numericId = Number(id);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.get<CaseDetailDto>(`${this.baseUrl}/${numericId}`).pipe(
      map(dto => this.mapDetailDtoToCase(dto)),
      catchError(err => throwError(() => err))
    );
  }

  createCase(apiDto: CreateCaseApiDto): Observable<Case> {
    // Ensure status/priority are in server-friendly labels if provided
    const payload: any = { ...apiDto };
    if (payload.status) {
      const s = String(payload.status);
      const enumMatch = Object.values(CaseStatus).find(v => v === s as any);
      payload.status = enumMatch ? this.mapCaseStatusToApi(enumMatch as CaseStatus) : payload.status;
    }
    if (payload.priority) {
      const p = String(payload.priority);
      const enumMatch = Object.values(CasePriority).find(v => v === p as any);
      payload.priority = enumMatch ? this.mapPriorityToApi(enumMatch as CasePriority) : payload.priority;
    }

    return this.http.post<CaseDetailDto>(this.baseUrl, payload).pipe(
      map(dto => this.mapDetailDtoToCase(dto)),
      catchError(err => throwError(() => err))
    );
  }

  updateCase(updateDto: UpdateCaseDto): Observable<Case> {
    const numericId = Number(updateDto.id);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    const apiDto = this.mapUpdateCaseToApi(updateDto);

    return this.http.put<CaseDetailDto>(`${this.baseUrl}/${numericId}`, apiDto).pipe(
      map(dto => this.mapDetailDtoToCase(dto)),
      catchError(err => throwError(() => err))
    );
  }

  deleteCase(id: string): Observable<boolean> {
    const numericId = Number(id);
    if (Number.isNaN(numericId)) {
      return throwError(() => new Error('Invalid case id'));
    }

    return this.http.delete<{ message: string; caseId: number }>(`${this.baseUrl}/${numericId}`).pipe(
      map(() => true),
      catchError(err => throwError(() => err))
    );
  }

  searchCases(searchTerm: string): Observable<Case[]> {
    if (!searchTerm) {
      return this.getCases();
    }

    const lower = searchTerm.toLowerCase();
    return this.getCases().pipe(
      map(cases =>
        cases.filter(c =>
          c.title.toLowerCase().includes(lower) ||
          c.description.toLowerCase().includes(lower)
        )
      )
    );
  }

  filterByStatus(status: CaseStatus): Observable<Case[]> {
    return this.getCases().pipe(
      map(cases => cases.filter(c => c.status === status))
    );
  }

  filterByPriority(priority: CasePriority): Observable<Case[]> {
    return this.getCases().pipe(
      map(cases => cases.filter(c => c.priority === priority))
    );
  }

  private mapSummaryDtoToCase(dto: CaseSummaryDto): Case {
    const createdAt = new Date(dto.dateFiled);
    const dueDate = undefined; // Summary DTO doesn't have due date
    return {
      id: dto.caseId.toString(),
      caseNumber: dto.caseNumber,
      title: dto.title,
      description: '',
      status: this.mapStatus(dto.status),
      priority: this.mapPriority(dto.priority),
      assignedTo: dto.lawyerName,
      lawyerName: dto.lawyerName,
      courtName: dto.courtName,
      dateFiled: createdAt,
      createdBy: dto.lawyerName,
      createdAt,
      updatedAt: createdAt,
      dueDate: undefined,
      tags: [],
      notes: undefined
    };
  }

  private mapDetailDtoToCase(dto: CaseDetailDto): Case {
    const createdAt = new Date(dto.createdAt);
    const updatedAt = dto.updatedAt ? new Date(dto.updatedAt) : createdAt;
    const lawyerName = dto.assignedLawyer?.fullName || '';
    const dateFiled = new Date(dto.dateFiled);
    const courtName = dto.court?.name ?? '';
    const dueDate = dto.endDate ? new Date(dto.endDate) : undefined;
    return {
      id: dto.caseId.toString(),
      caseNumber: dto.caseNumber,
      title: dto.title,
      description: dto.description ?? '',
      status: this.mapStatus(dto.status),
      priority: this.mapPriority(dto.priority),
      assignedTo: lawyerName || undefined,
      lawyerName: lawyerName || undefined,
      courtName,
      dateFiled,
      createdBy: lawyerName,
      createdAt,
      updatedAt,
      dueDate,
      tags: [],
      notes: dto.outcome ?? undefined
    } as Case;
  }

  private mapStatus(status: string): CaseStatus {
    const normalized = (status || '').toLowerCase();
    if (normalized.includes('pending')) return CaseStatus.PENDING;
    if (normalized.includes('progress')) return CaseStatus.IN_PROGRESS;
    if (normalized.includes('closed') || normalized.includes('complete')) return CaseStatus.CLOSED;
    if (normalized.includes('resolved')) return CaseStatus.RESOLVED;
    return CaseStatus.OPEN;
  }

  private mapPriority(priority: string): CasePriority {
    const normalized = priority.toLowerCase();
    if (normalized.includes('low')) return CasePriority.LOW;
    if (normalized.includes('high') && !normalized.includes('critical')) return CasePriority.HIGH;
    if (normalized.includes('critical')) return CasePriority.CRITICAL;
    return CasePriority.MEDIUM;
  }

  private mapUpdateCaseToApi(dto: UpdateCaseDto): any {
    // Map frontend UpdateCaseDto to backend UpdateCaseDto shape.
    // Only send fields that the backend understands and that we actually have.
    const apiDto: any = {};

    if (dto.title !== undefined) {
      apiDto.title = dto.title;
    }
    if (dto.description !== undefined) {
      apiDto.description = dto.description;
    }
    if (dto.status !== undefined) {
      apiDto.status = this.mapCaseStatusToApi(dto.status);
    }
    if (dto.dueDate !== undefined) {
      apiDto.endDate = dto.dueDate.toISOString();
    }
    if (dto.notes !== undefined) {
      apiDto.outcome = dto.notes;
    }
    if ((dto as any).priority !== undefined) {
      // Map priority back to server label if provided
      apiDto.priority = this.mapPriorityToApi((dto as any).priority as CasePriority);
    }

    return apiDto;
  }

  private mapCaseStatusToApi(status: CaseStatus): string {
    switch (status) {
      case CaseStatus.PENDING:
        return 'Pending';
      case CaseStatus.IN_PROGRESS:
        return 'In Progress';
      case CaseStatus.RESOLVED:
        return 'Resolved';
      case CaseStatus.CLOSED:
        return 'Completed';
      default:
        return 'Open';
    }
  }

  private mapPriorityToApi(priority: CasePriority): string {
    switch (priority) {
      case CasePriority.LOW:
        return 'Low';
      case CasePriority.HIGH:
        return 'High';
      case CasePriority.CRITICAL:
        return 'Critical';
      default:
        return 'Medium';
    }
  }

  // Workflow methods
  getCaseWorkflow(caseId: number): Observable<CaseWorkflow> {
    return this.http.get<CaseWorkflow>(`${this.baseUrl}/${caseId}/workflow`).pipe(
      catchError(err => throwError(() => err))
    );
  }
}