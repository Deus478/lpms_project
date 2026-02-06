export interface MeetingSummaryDto {
  meetingId: number;
  boardId?: number | null;
  committeeId?: number | null;
  scheduledDate: string;
  title?: string | null;
  status: string;
  location?: string | null;
  attendanceCount: number;
}

export interface CreateMeetingDto {
  boardId?: number | null;
  committeeId?: number | null;
  scheduledDate: string;
  location?: string | null;
  title?: string | null;
  agenda?: string | null;
}

export interface UpdateMeetingDto {
  scheduledDate?: string | null;
  location?: string | null;
  title?: string | null;
  agenda?: string | null;
  status?: string | null;
}

export interface CreateMinuteDto {
  content: string;
  documentId?: string | null;
}

export interface MinuteDto {
  minuteId: number;
  meetingId: number;
  recordedDate: string;
  content: string;
  documentId?: string | null;
}

export interface UpdateMinuteDocumentDto {
  documentId?: string | null;
}

export interface CreateResolutionDto {
  title: string;
  description?: string | null;
  dueDate?: string | null;
  responsibleParty?: string | null;
}

export interface UpdateResolutionStatusDto {
  status: string;
  completedAt?: string | null;
}

export interface ResolutionDto {
  resolutionId: number;
  meetingId: number;
  title: string;
  description?: string | null;
  status: string;
  dueDate?: string | null;
  responsibleParty?: string | null;
  createdAt: string;
  completedAt?: string | null;
}

export interface AttendanceItemDto {
  attendeeName: string;
  attendeeRole?: string | null;
  present: boolean;
  notes?: string | null;
}

export interface RecordAttendanceDto {
  items: AttendanceItemDto[];
}

export interface CreateRecurringMeetingDto {
  boardId?: number | null;
  committeeId?: number | null;
  startDate: string;
  location?: string | null;
  title?: string | null;
  agenda?: string | null;
  intervalWeeks: number;
  daysOfWeek: number[];
  endDate?: string | null;
  occurrences?: number | null;
}

export interface RecurringSeriesResultDto {
  seriesId: string;
  createdMeetings: MeetingSummaryDto[];
}
