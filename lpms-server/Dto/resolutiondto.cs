using System;

namespace LegalCaseManagement.DTOs
{
    public class CreateResolutionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ResponsibleParty { get; set; }
    }

    public class UpdateResolutionStatusDto
    {
        public string Status { get; set; } = string.Empty; // Pending, InProgress, Completed
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
        public string? ResponsibleParty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}


