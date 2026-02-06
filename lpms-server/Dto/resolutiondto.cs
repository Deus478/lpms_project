using System;

namespace LegalCaseManagement.DTOs
{
    public class CreateResolutionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int ResponsibleUserId { get; set; }
        public string? ActionItems { get; set; }
        public string? Department { get; set; }
        public int Priority { get; set; } = 1;
        public bool RequiresBoardFollowUp { get; set; } = false;
    }

    public class UpdateResolutionStatusDto
    {
        public string Status { get; set; } = string.Empty; // Pending, InProgress, Completed, Overdue
        public DateTime? CompletedAt { get; set; }
    }

    public class ResolutionDto
    {
        public int ResolutionId { get; set; }
        public int MeetingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int ResponsibleUserId { get; set; }
        public string? ActionItems { get; set; }
        public string? Department { get; set; }
        public int Priority { get; set; }
        public bool RequiresBoardFollowUp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}


