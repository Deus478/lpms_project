using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a workflow instance for a specific case
    /// </summary>
    public class CaseWorkflow
    {
        [Key]
        public int CaseWorkflowId { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public int WorkflowTemplateId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed, Cancelled

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey("WorkflowTemplateId")]
        public virtual WorkflowTemplate WorkflowTemplate { get; set; } = null!;

        public virtual ICollection<CaseWorkflowStep> Steps { get; set; } = new List<CaseWorkflowStep>();
    }
}
