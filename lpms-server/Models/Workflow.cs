using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a workflow template for case processing
    /// </summary>
    public class WorkflowTemplate
    {
        [Key]
        public int WorkflowTemplateId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string CaseType { get; set; } = string.Empty; // litigation, recovery, labor, regulatory, contractual

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<WorkflowStepTemplate> StepTemplates { get; set; } = new List<WorkflowStepTemplate>();
    }

    /// <summary>
    /// Represents a step in a workflow template
    /// </summary>
    public class WorkflowStepTemplate
    {
        [Key]
        public int WorkflowStepTemplateId { get; set; }

        [Required]
        public int WorkflowTemplateId { get; set; }

        [Required]
        [StringLength(200)]
        public string StepName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int StepOrder { get; set; }

        [Required]
        [StringLength(50)]
        public string ApproverRole { get; set; } = string.Empty; // LegalOfficer, HeadOfLegal, ExecutiveManagement, BoardCommittee

        [StringLength(20)]
        public string Action { get; set; } = "Review"; // Review, Approve, Reject, Escalate

        public int? TimeoutHours { get; set; }

        public bool IsOptional { get; set; } = true;

        [ForeignKey("WorkflowTemplateId")]
        public virtual WorkflowTemplate WorkflowTemplate { get; set; } = null!;
    }
}
