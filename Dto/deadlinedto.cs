using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for deadline information
    /// </summary>
    public class DeadlineDto
    {
        public int DeadlineId { get; set; }
        public int CaseId { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new deadline
    /// </summary>
    public class CreateDeadlineDto
    {
        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string Priority { get; set; } = "Medium";

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating a deadline
    /// </summary>
    public class UpdateDeadlineDto
    {
        public DateTime? DueDate { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string? Priority { get; set; }

        public bool? IsCompleted { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}