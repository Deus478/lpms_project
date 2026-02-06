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

        public DateTime? EndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string MeetingType { get; set; } = "Board"; // Board, BoardCommittee, ExecutiveManagement, LegalReview, AdHoc

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Agenda { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

        public int? ChairpersonUserId { get; set; }

        public int? SecretaryUserId { get; set; }

        public int? MinQuorum { get; set; } // Minimum number of participants required

        public Guid? SeriesId { get; set; }
        [MaxLength(50)]
        public string? RecurrenceType { get; set; }
        public int? RecurrenceInterval { get; set; }
        [MaxLength(50)]
        public string? RecurrenceDays { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }

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

        [ForeignKey("ChairpersonUserId")]
        public virtual User? ChairpersonUser { get; set; }

        [ForeignKey("SecretaryUserId")]
        public virtual User? SecretaryUser { get; set; }
    }
}


