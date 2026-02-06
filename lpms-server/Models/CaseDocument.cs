using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents documents associated with a legal case
    /// </summary>
    public class CaseDocument
    {
        [Key]
        public int CaseDocumentId { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Pleading, Evidence, Contract, Correspondence, CourtOrder, etc.

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public int? AddedByUserId { get; set; }

        public bool IsConfidential { get; set; } = false;

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } = null!;

        [ForeignKey("AddedByUserId")]
        public virtual User? AddedByUser { get; set; }
    }
}
