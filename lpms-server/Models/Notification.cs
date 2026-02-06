using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a notification in the LCMS system
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // CaseHearing, ContractRenewal, ApprovalDelay, MeetingAction, RetentionExpiry

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [StringLength(100)]
        public string? EntityType { get; set; } // Case, Contract, Meeting, Document

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
