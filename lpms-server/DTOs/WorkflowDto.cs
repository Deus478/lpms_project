using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    public class WorkflowTemplateDto
    {
        public int WorkflowTemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CaseType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WorkflowStepTemplateDto> StepTemplates { get; set; } = new();
    }

    public class WorkflowStepTemplateDto
    {
        public int WorkflowStepTemplateId { get; set; }
        public int WorkflowTemplateId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StepOrder { get; set; }
        public string ApproverRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? TimeoutHours { get; set; }
        public bool IsOptional { get; set; }
    }

    public class CreateWorkflowTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string CaseType { get; set; } = string.Empty;

        [Required]
        public List<CreateWorkflowStepTemplateDto> Steps { get; set; } = new();
    }

    public class CreateWorkflowStepTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string StepName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int StepOrder { get; set; }

        [Required]
        [StringLength(50)]
        public string ApproverRole { get; set; } = string.Empty;

        [StringLength(50)]
        public string Action { get; set; } = "Review";

        public int? TimeoutHours { get; set; }

        public bool IsOptional { get; set; } = false;
    }

    public class CaseWorkflowDto
    {
        public int CaseWorkflowId { get; set; }
        public int CaseId { get; set; }
        public int WorkflowTemplateId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public WorkflowTemplateDto WorkflowTemplate { get; set; } = null!;
        public List<CaseWorkflowStepDto> WorkflowSteps { get; set; } = new();
    }

    public class CaseWorkflowStepDto
    {
        public int CaseWorkflowStepId { get; set; }
        public int CaseWorkflowId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string ApproverRole { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? AssignedUserId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Comments { get; set; }
        public string? LegalOpinion { get; set; }
        public UserDto? AssignedUser { get; set; }
    }

    public class StartWorkflowDto
    {
        [Required]
        public int WorkflowTemplateId { get; set; }
    }

    public class ApproveStepDto
    {
        [Required]
        [StringLength(20)]
        public string Action { get; set; } = string.Empty; // Approved, Rejected

        [StringLength(2000)]
        public string? Comments { get; set; }

        [StringLength(1000)]
        public string? LegalOpinion { get; set; }
    }

    public class AssignStepDto
    {
        [Required]
        public int UserId { get; set; }
    }

    public class WorkflowTaskDto
    {
        public int CaseWorkflowStepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string CaseNumber { get; set; } = string.Empty;
        public string CaseTitle { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CaseId { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
    }
}
