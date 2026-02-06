using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a step in a case workflow
    /// </summary>
    public class CaseWorkflowStep
    {
        [Key]
        public int CaseWorkflowStepId { get; set; }

        [Required]
        public int CaseWorkflowId { get; set; }

        [Required]
        public int WorkflowStepTemplateId { get; set; }

        [Required]
        [StringLength(200)]
        public string StepName { get; set; } = string.Empty;

        [Required]
        public int StepOrder { get; set; }

        [Required]
        [StringLength(50)]
        public string ApproverRole { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Skipped

        public int? AssignedUserId { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }

        [StringLength(2000)]
        public string? Comments { get; set; }

        [StringLength(1000)]
        public string? LegalOpinion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CaseWorkflowId")]
        public virtual CaseWorkflow CaseWorkflow { get; set; } = null!;

        [ForeignKey("WorkflowStepTemplateId")]
        public virtual WorkflowStepTemplate WorkflowStepTemplate { get; set; } = null!;

        [ForeignKey("AssignedUserId")]
        public virtual User? AssignedUser { get; set; }
    }
}
