using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a hearing scheduled for a case
    /// </summary>
    public class Hearing
    {
        [Key]
        public int HearingId { get; set; }

        [Required]
        public int CaseId { get; set; }

        public int? CourtId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? HearingType { get; set; } // Initial, Follow-up, Final, etc.

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Postponed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey("CourtId")]
        public virtual Court? Court { get; set; }
    }
}