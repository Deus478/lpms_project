using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a scheduled meeting for a Board or Committee
    /// </summary>
    public class Meeting
    {
        [Key]
        public int MeetingId { get; set; }

        public int? BoardId { get; set; }
        public int? CommitteeId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Agenda { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        [ForeignKey("CommitteeId")]
        public virtual Committee? Committee { get; set; }

        // Navigation
        public virtual ICollection<MeetingAttendance> Attendances { get; set; } = new List<MeetingAttendance>();
        public virtual ICollection<Minute> Minutes { get; set; } = new List<Minute>();
        public virtual ICollection<Resolution> Resolutions { get; set; } = new List<Resolution>();
    }
}


