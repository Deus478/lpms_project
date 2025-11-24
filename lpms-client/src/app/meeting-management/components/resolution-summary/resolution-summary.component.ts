import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MeetingService } from '../../services/meeting.service';

@Component({
  selector: 'app-resolution-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './resolution-summary.component.html',
  styleUrls: ['./resolution-summary.component.css']
})
export class ResolutionSummaryComponent implements OnInit {
  summary: { total: number; pending: number; inProgress: number; completed: number; overdue: number } | null = null;
  isLoading = false;

  constructor(private meetingService: MeetingService) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.isLoading = true;
    this.meetingService.getResolutionSummary().subscribe({
      next: data => {
        this.summary = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }
}
