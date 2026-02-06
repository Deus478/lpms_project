using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents contract renewal information and tracking
    /// </summary>
    public class ContractRenewal
    {
        [Key]
        public int ContractRenewalId { get; set; }

        [Required]
        public int ContractId { get; set; }

        public DateTime RenewalDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string RenewalType { get; set; } = string.Empty; // Automatic, Manual, Review

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed

        [StringLength(1000)]
        public string? Terms { get; set; }

        public decimal? NewValue { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? ReviewNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual User? ReviewedByUser { get; set; } = null!;
    }
}
