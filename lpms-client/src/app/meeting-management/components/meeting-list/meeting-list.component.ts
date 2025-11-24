import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { MeetingSummaryDto } from '../../models/meeting-api.model';

@Component({
  selector: 'app-meeting-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './meeting-list.component.html',
  styleUrls: ['./meeting-list.component.css']
})
export class MeetingListComponent implements OnInit {
  meetings: MeetingSummaryDto[] = [];
  filteredMeetings: MeetingSummaryDto[] = [];
  isLoading = false;
  searchTerm = '';
  selectedStatus: string = 'ALL';
  statusOptions: string[] = [];

  constructor(
    private meetingService: MeetingService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.isLoading = true;
    this.meetingService.getMeetings().subscribe({
      next: meetings => {
        this.meetings = meetings;
        this.filteredMeetings = meetings;
        const uniqueStatuses = Array.from(new Set(meetings.map(m => m.status))).filter(s => s);
        this.statusOptions = uniqueStatuses;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredMeetings = this.meetings.filter(m => {
      const matchesSearch = !term ||
        (m.title && m.title.toLowerCase().includes(term)) ||
        (m.location && m.location.toLowerCase().includes(term));
      const matchesStatus = this.selectedStatus === 'ALL' || m.status === this.selectedStatus;
      return matchesSearch && matchesStatus;
    });
  }

  viewMeeting(meeting: MeetingSummaryDto): void {
    this.router.navigate(['/meetings', meeting.meetingId]);
  }

  editMeeting(meeting: MeetingSummaryDto): void {
    this.router.navigate(['/meetings/edit', meeting.meetingId]);
  }

  createMeeting(): void {
    this.router.navigate(['/meetings/create']);
  }

  formatDate(value: string): string {
    return this.meetingService.formatDate(value);
  }

  viewResolutionSummary(): void {
    this.router.navigate(['/meetings/resolution-summary']);
  }
}
