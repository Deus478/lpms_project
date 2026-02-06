using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents an audit trail entry for compliance and traceability
    /// </summary>
    public class AuditTrail
    {
        [Key]
        public int AuditTrailId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Approve, Reject, Login, Logout

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // Case, Contract, Document, Meeting, User, etc.

        public int? EntityId { get; set; }

        [StringLength(100)]
        public string? EntityIdentifier { get; set; } // CaseNumber, ContractNumber, etc.

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? OldValues { get; set; } // JSON string of old values

        public string? NewValues { get; set; } // JSON string of new values

        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsSensitive { get; set; } = false; // Mark sensitive operations for additional protection

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
