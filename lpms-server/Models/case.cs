using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a legal case in the system
    /// </summary>
    public class Case
    {
        [Key]
        public int CaseId { get; set; }

        [Required]
        [StringLength(50)]
        public string CaseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int AssignedLawyerId { get; set; }

        // Side-specific attorneys
        public int? PlaintiffLawyerId { get; set; }
        public int? DefendantLawyerId { get; set; }

        [Required]
        public int CourtId { get; set; }

        [Required]
        public DateTime DateFiled { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed

        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [StringLength(500)]
        public string? Outcome { get; set; }

        public bool IsActive { get; set; } = true; // For soft delete

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Lawyer AssignedLawyer { get; set; } = null!;
        public virtual Lawyer? PlaintiffLawyer { get; set; }
        public virtual Lawyer? DefendantLawyer { get; set; }
        public virtual Court Court { get; set; } = null!;
        public virtual ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
        public virtual ICollection<Deadline> Deadlines { get; set; } = new List<Deadline>();
        public virtual ICollection<CaseParty> CaseParties { get; set; } = new List<CaseParty>();
        public virtual ICollection<CaseLawyer> CaseLawyers { get; set; } = new List<CaseLawyer>();
    }
}