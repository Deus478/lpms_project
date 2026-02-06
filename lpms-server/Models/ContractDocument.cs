using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents documents associated with contracts
    /// </summary>
    public class ContractDocument
    {
        [Key]
        public int ContractDocumentId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Draft, Final, Signed, Amendment, etc.

        public int Version { get; set; } = 1;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } = null!;
    }
}
