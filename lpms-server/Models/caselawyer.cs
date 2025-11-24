using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Junction table for many-to-many relationship between Case and Lawyer (additional lawyers)
    /// </summary>
    public class CaseLawyer
    {
        [Key]
        public int CaseLawyerId { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public int LawyerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey("LawyerId")]
        public virtual Lawyer Lawyer { get; set; } = null!;
    }
}
