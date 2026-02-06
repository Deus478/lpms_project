using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    public class ContractDto
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ContractType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int RequestingDepartmentId { get; set; }
        public int? ClientId { get; set; }
        public int? AssignedLawyerId { get; set; }
        public string? Counterparty { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public bool AutoRenew { get; set; }
        public int? RenewalNoticeDays { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ContractDocumentDto> Documents { get; set; } = new();
        public List<ContractApprovalDto> Approvals { get; set; } = new();
    }

    public class CreateContractDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractType { get; set; } = string.Empty;

        [Required]
        public int RequestingDepartmentId { get; set; }

        public int? ClientId { get; set; }

        public int? AssignedLawyerId { get; set; }

        [StringLength(500)]
        public string? Counterparty { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ContractValue { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool AutoRenew { get; set; } = false;

        public int? RenewalNoticeDays { get; set; }

        [StringLength(20)]
        public string RiskLevel { get; set; } = "Medium";
    }

    public class UpdateContractStatusDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }

    public class ContractDocumentDto
    {
        public int ContractDocumentId { get; set; }
        public int ContractId { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Description { get; set; }
    }

    public class AddContractDocumentDto
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class ContractApprovalDto
    {
        public int ContractApprovalId { get; set; }
        public int ContractId { get; set; }
        public int ApproverUserId { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public UserDto? ApproverUser { get; set; }
    }

    public class SubmitForApprovalDto
    {
        [Required]
        public int ApproverUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApprovalLevel { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Comments { get; set; }
    }

    public class ContractRenewalDto
    {
        public int ContractRenewalId { get; set; }
        public int ContractId { get; set; }
        public DateTime RenewalDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string RenewalType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Terms { get; set; }
        public decimal? NewValue { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
