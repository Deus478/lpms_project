import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  MeetingSummaryDto,
  CreateMeetingDto,
  UpdateMeetingDto,
  MinuteDto,
  CreateMinuteDto,
  ResolutionDto,
  CreateResolutionDto,
  UpdateResolutionStatusDto,
  RecordAttendanceDto,
  CreateRecurringMeetingDto,
  RecurringSeriesResultDto
} from '../models/meeting-api.model';

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/meetings`;
  private readonly reportsUrl = `${environment.apiBaseUrl}/api/reports`;

  private meetingsSubject = new BehaviorSubject<MeetingSummaryDto[]>([]);
  meetings$ = this.meetingsSubject.asObservable();

  constructor(private http: HttpClient) {}

  getMeetings(): Observable<MeetingSummaryDto[]> {
    return this.http.get<MeetingSummaryDto[]>(this.apiUrl).pipe(
      tap(list => this.meetingsSubject.next(list)),
      catchError(err => throwError(() => err))
    );
  }

  getMeetingById(id: number): Observable<MeetingSummaryDto | undefined> {
    const current = this.meetingsSubject.value;
    if (current.length > 0) {
      return of(current.find(m => m.meetingId === id));
    }
    return this.getMeetings().pipe(
      map(list => list.find(m => m.meetingId === id))
    );
  }

  createMeeting(dto: CreateMeetingDto): Observable<MeetingSummaryDto> {
    return this.http.post<MeetingSummaryDto>(this.apiUrl, dto).pipe(
      tap(created => {
        const current = this.meetingsSubject.value;
        this.meetingsSubject.next([created, ...current]);
      }),
      catchError(err => throwError(() => err))
    );
  }

  updateMeeting(id: number, dto: UpdateMeetingDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}`, dto).pipe(
      tap(() => {
        const current = this.meetingsSubject.value;
        const index = current.findIndex(m => m.meetingId === id);
        if (index !== -1) {
          const updated = { ...current[index], ...dto } as MeetingSummaryDto;
          const copy = [...current];
          copy[index] = updated;
          this.meetingsSubject.next(copy);
        }
      }),
      catchError(err => throwError(() => err))
    );
  }

  getMinutes(meetingId: number): Observable<MinuteDto[]> {
    return this.http.get<MinuteDto[]>(`${this.apiUrl}/${meetingId}/minutes`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  addMinute(meetingId: number, dto: CreateMinuteDto): Observable<MinuteDto> {
    return this.http.post<MinuteDto>(`${this.apiUrl}/${meetingId}/minutes`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getResolutions(meetingId: number): Observable<ResolutionDto[]> {
    return this.http.get<ResolutionDto[]>(`${this.apiUrl}/${meetingId}/resolutions`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  addResolution(meetingId: number, dto: CreateResolutionDto): Observable<ResolutionDto> {
    return this.http.post<ResolutionDto>(`${this.apiUrl}/${meetingId}/resolutions`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  updateResolutionStatus(meetingId: number, resolutionId: number, dto: UpdateResolutionStatusDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${meetingId}/resolutions/${resolutionId}/status`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  recordAttendance(meetingId: number, dto: RecordAttendanceDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${meetingId}/attendance`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  createRecurringMeetings(dto: CreateRecurringMeetingDto): Observable<RecurringSeriesResultDto> {
    return this.http.post<RecurringSeriesResultDto>(`${this.apiUrl}/recurring`, dto).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getResolutionSummary(): Observable<any> {
    return this.http.get<any>(`${this.reportsUrl}/resolutions/summary`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  formatDate(value: string): string {
    if (!value) {
      return '';
    }
    return new Date(value).toLocaleString();
  }
}
