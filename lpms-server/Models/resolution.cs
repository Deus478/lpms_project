using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Resolution passed in a meeting, with implementation tracking
    /// </summary>
    public class Resolution
    {
        [Key]
        public int ResolutionId { get; set; }

        [Required]
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Overdue

        public DateTime? DueDate { get; set; }

        [Required]
        public int ResponsibleUserId { get; set; }

        [MaxLength(1000)]
        public string? ActionItems { get; set; }

        [MaxLength(500)]
        public string? Department { get; set; }

        public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical

        public bool RequiresBoardFollowUp { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("MeetingId")]
        public virtual Meeting Meeting { get; set; } = null!;

        [ForeignKey("ResponsibleUserId")]
        public virtual User ResponsibleUser { get; set; } = null!;
    }
}


