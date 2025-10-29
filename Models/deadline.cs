using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a deadline associated with a case
    /// </summary>
    public class Deadline
    {
        [Key]
        public int DeadlineId { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;
    }
}