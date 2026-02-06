using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a contract in the end-to-end contract management system
    /// </summary>
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        [StringLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractType { get; set; } = string.Empty; // Service, Supply, Partnership, Employment, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, UnderReview, Approved, Executed, Active, Expired, Terminated

        [Required]
        public int RequestingDepartmentId { get; set; }

        public int? ClientId { get; set; }

        public int? AssignedLawyerId { get; set; }

        [StringLength(500)]
        public string? Counterparty { get; set; }

        [Required]
        public decimal ContractValue { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public bool AutoRenew { get; set; } = false;

        public int? RenewalNoticeDays { get; set; }

        [StringLength(20)]
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High, Critical

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("ClientId")]
        public virtual Party? Client { get; set; }

        [ForeignKey("AssignedLawyerId")]
        public virtual User? AssignedLawyer { get; set; }

        public virtual ICollection<ContractDocument> ContractDocuments { get; set; } = new List<ContractDocument>();
        public virtual ICollection<ContractApproval> ContractApprovals { get; set; } = new List<ContractApproval>();
        public virtual ICollection<ContractRenewal> ContractRenewals { get; set; } = new List<ContractRenewal>();
    }
}
