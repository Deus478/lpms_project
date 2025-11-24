import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { CreateMeetingDto, UpdateMeetingDto } from '../../models/meeting-api.model';

@Component({
  selector: 'app-meeting-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './meeting-form.component.html',
  styleUrls: ['./meeting-form.component.css']
})
export class MeetingFormComponent implements OnInit {
  isEditMode = false;
  meetingId?: number;

  title = '';
  boardId?: number;
  committeeId?: number;
  scheduledDateLocal = '';
  location = '';
  agenda = '';
  status = 'Scheduled';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingService: MeetingService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.meetingId = Number(idParam);
      if (!Number.isNaN(this.meetingId)) {
        this.loadMeeting(this.meetingId);
      }
    }
  }

  loadMeeting(id: number): void {
    this.meetingService.getMeetingById(id).subscribe({
      next: meeting => {
        if (!meeting) {
          return;
        }
        this.title = meeting.title || '';
        this.boardId = meeting.boardId || undefined;
        this.committeeId = meeting.committeeId || undefined;
        this.location = meeting.location || '';
        this.agenda = '';
        this.status = meeting.status || 'Scheduled';
        this.scheduledDateLocal = this.toLocalDateTimeInput(meeting.scheduledDate);
      },
      error: () => {}
    });
  }

  toLocalDateTimeInput(value: string): string {
    const d = new Date(value);
    const pad = (n: number) => n.toString().padStart(2, '0');
    const year = d.getFullYear();
    const month = pad(d.getMonth() + 1);
    const day = pad(d.getDate());
    const hours = pad(d.getHours());
    const minutes = pad(d.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  onSubmit(): void {
    if (!this.scheduledDateLocal) {
      return;
    }
    const scheduledDateIso = new Date(this.scheduledDateLocal).toISOString();

    if (this.isEditMode && this.meetingId) {
      const dto: UpdateMeetingDto = {
        title: this.title || undefined,
        location: this.location || undefined,
        agenda: this.agenda || undefined,
        status: this.status || undefined,
        scheduledDate: scheduledDateIso
      };
      this.meetingService.updateMeeting(this.meetingId, dto).subscribe({
        next: () => this.router.navigate(['/meetings']),
        error: () => {}
      });
    } else {
      const dto: CreateMeetingDto = {
        boardId: this.boardId,
        committeeId: this.committeeId,
        scheduledDate: scheduledDateIso,
        location: this.location || undefined,
        title: this.title || undefined,
        agenda: this.agenda || undefined
      };
      this.meetingService.createMeeting(dto).subscribe({
        next: () => this.router.navigate(['/meetings']),
        error: () => {}
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/meetings']);
  }
}
