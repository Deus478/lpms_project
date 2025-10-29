using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Junction table for many-to-many relationship between Case and Party
    /// </summary>
    public class CaseParty
    {
        [Key]
        public int CasePartyId { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public int PartyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // Plaintiff, Defendant, Witness, etc.

        [Required]
        [StringLength(20)]
        public string Side { get; set; } = "Accuser"; // Accuser or Accused

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey("PartyId")]
        public virtual Party Party { get; set; } = null!;
    }
}