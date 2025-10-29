using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Tracks attendance for a meeting
    /// </summary>
    public class MeetingAttendance
    {
        [Key]
        public int MeetingAttendanceId { get; set; }

        [Required]
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(200)]
        public string AttendeeName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AttendeeRole { get; set; } // e.g., Director, Secretary

        public bool Present { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("MeetingId")]
        public virtual Meeting Meeting { get; set; } = null!;
    }
}


