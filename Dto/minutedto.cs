using System;

namespace LegalCaseManagement.DTOs
{
    public class CreateMinuteDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid? DocumentId { get; set; }
    }

    public class MinuteDto
    {
        public int MinuteId { get; set; }
        public int MeetingId { get; set; }
        public DateTime RecordedDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? DocumentId { get; set; }
    }
}


