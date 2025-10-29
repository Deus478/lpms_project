using System;
using System.Collections.Generic;

namespace LegalCaseManagement.DTOs
{
    public class CreateMeetingDto
    {
        public int? BoardId { get; set; }
        public int? CommitteeId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? Location { get; set; }
        public string? Title { get; set; }
        public string? Agenda { get; set; }
    }

    public class UpdateMeetingDto
    {
        public DateTime? ScheduledDate { get; set; }
        public string? Location { get; set; }
        public string? Title { get; set; }
        public string? Agenda { get; set; }
        public string? Status { get; set; }
    }

    public class MeetingSummaryDto
    {
        public int MeetingId { get; set; }
        public int? BoardId { get; set; }
        public int? CommitteeId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? Title { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int AttendanceCount { get; set; }
    }

    public class RecordAttendanceDto
    {
        public List<AttendanceItemDto> Items { get; set; } = new();
    }

    public class AttendanceItemDto
    {
        public string AttendeeName { get; set; } = string.Empty;
        public string? AttendeeRole { get; set; }
        public bool Present { get; set; } = true;
        public string? Notes { get; set; }
    }
}


