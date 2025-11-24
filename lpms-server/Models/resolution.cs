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

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        public DateTime? DueDate { get; set; }

        [MaxLength(500)]
        public string? ResponsibleParty { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        [ForeignKey("MeetingId")]
        public virtual Meeting Meeting { get; set; } = null!;
    }
}


