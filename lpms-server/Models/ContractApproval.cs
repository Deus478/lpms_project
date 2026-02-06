using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents approval workflow steps for contracts
    /// </summary>
    public class ContractApproval
    {
        [Key]
        public int ContractApprovalId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        public int ApproverUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApprovalLevel { get; set; } = string.Empty; // Manager, Director, Legal, Executive

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [ForeignKey("ApproverUserId")]
        public virtual User ApproverUser { get; set; } = null!;
    }
}
