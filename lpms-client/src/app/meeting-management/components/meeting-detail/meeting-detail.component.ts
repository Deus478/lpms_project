import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { MeetingSummaryDto, MinuteDto, ResolutionDto, CreateMinuteDto, CreateResolutionDto, AttendanceItemDto, RecordAttendanceDto } from '../../models/meeting-api.model';

@Component({
  selector: 'app-meeting-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './meeting-detail.component.html',
  styleUrls: ['./meeting-detail.component.css']
})
export class MeetingDetailComponent implements OnInit {
  meeting?: MeetingSummaryDto;
  minutes: MinuteDto[] = [];
  resolutions: ResolutionDto[] = [];
  isLoading = false;

  newMinuteContent = '';
  newMinuteDocumentId = '';

  newResolutionTitle = '';
  newResolutionDescription = '';
  newResolutionDueDate = '';
  newResolutionResponsibleParty = '';

  attendanceItems: AttendanceItemDto[] = [];
  newAttendeeName = '';
  newAttendeeRole = '';
  newAttendeePresent = true;
  newAttendeeNotes = '';

  constructor(
    private route: ActivatedRoute,
    private meetingService: MeetingService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;
    if (Number.isNaN(id)) {
      return;
    }
    this.loadAll(id);
  }

  loadAll(id: number): void {
    this.isLoading = true;
    this.meetingService.getMeetingById(id).subscribe({
      next: meeting => {
        this.meeting = meeting;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });

    this.loadMinutes(id);
    this.loadResolutions(id);
  }

  loadMinutes(meetingId: number): void {
    this.meetingService.getMinutes(meetingId).subscribe({
      next: list => {
        this.minutes = list;
      },
      error: () => {}
    });
  }

  loadResolutions(meetingId: number): void {
    this.meetingService.getResolutions(meetingId).subscribe({
      next: list => {
        this.resolutions = list;
      },
      error: () => {}
    });
  }

  addMinute(): void {
    if (!this.meeting || !this.newMinuteContent.trim()) {
      return;
    }
    const trimmedId = (this.newMinuteDocumentId || '').trim();
    const dto: CreateMinuteDto = {
      content: this.newMinuteContent.trim(),
      documentId: trimmedId && this.isGuid(trimmedId) ? trimmedId : undefined
    };
    this.meetingService.addMinute(this.meeting.meetingId, dto).subscribe({
      next: minute => {
        this.minutes = [...this.minutes, minute];
        this.newMinuteContent = '';
        this.newMinuteDocumentId = '';
      },
      error: () => {}
    });
  }

  addResolution(): void {
    if (!this.meeting || !this.newResolutionTitle.trim()) {
      return;
    }
    const dto: CreateResolutionDto = {
      title: this.newResolutionTitle.trim(),
      description: this.newResolutionDescription || undefined,
      dueDate: this.newResolutionDueDate ? new Date(this.newResolutionDueDate).toISOString() : undefined,
      responsibleParty: this.newResolutionResponsibleParty || undefined
    };
    this.meetingService.addResolution(this.meeting.meetingId, dto).subscribe({
      next: res => {
        this.resolutions = [...this.resolutions, res];
        this.newResolutionTitle = '';
        this.newResolutionDescription = '';
        this.newResolutionDueDate = '';
        this.newResolutionResponsibleParty = '';
      },
      error: () => {}
    });
  }

  setResolutionStatus(resolution: ResolutionDto, status: string): void {
    if (!this.meeting) {
      return;
    }
    const dto = {
      status,
      completedAt: status === 'Completed' ? new Date().toISOString() : null
    };
    this.meetingService.updateResolutionStatus(this.meeting.meetingId, resolution.resolutionId, dto).subscribe({
      next: () => {
        this.resolutions = this.resolutions.map(r =>
          r.resolutionId === resolution.resolutionId ? { ...r, status } : r
        );
      },
      error: () => {}
    });
  }

  addAttendanceItem(): void {
    const name = this.newAttendeeName.trim();
    if (!name) {
      return;
    }

    const item: AttendanceItemDto = {
      attendeeName: name,
      attendeeRole: this.newAttendeeRole || undefined,
      present: this.newAttendeePresent,
      notes: this.newAttendeeNotes || undefined
    };

    this.attendanceItems = [...this.attendanceItems, item];
    this.newAttendeeName = '';
    this.newAttendeeRole = '';
    this.newAttendeePresent = true;
    this.newAttendeeNotes = '';
  }

  removeAttendanceItem(index: number): void {
    if (index < 0 || index >= this.attendanceItems.length) {
      return;
    }
    const copy = [...this.attendanceItems];
    copy.splice(index, 1);
    this.attendanceItems = copy;
  }

  saveAttendance(): void {
    if (!this.meeting || this.attendanceItems.length === 0) {
      return;
    }

    const meeting = this.meeting as MeetingSummaryDto;
    const dto: RecordAttendanceDto = {
      items: this.attendanceItems
    };

    this.meetingService.recordAttendance(meeting.meetingId, dto).subscribe({
      next: () => {
        this.attendanceItems = [];
        const updated: MeetingSummaryDto = {
          ...meeting,
          attendanceCount: (meeting.attendanceCount ?? 0) + dto.items.length
        };
        this.meeting = updated;
      },
      error: () => {}
    });
  }

  formatDate(value: string | null | undefined): string {
    if (!value) {
      return '';
    }
    return this.meetingService.formatDate(value);
  }

  displayResolutionStatus(status: string | null | undefined): string {
    if (!status) return '';
    if (status === 'InProgress') return 'In Progress';
    return status;
  }

  private isGuid(str: string): boolean {
    return /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/.test(str);
  }
}
