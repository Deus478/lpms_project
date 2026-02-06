using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    public class Judge
    {
        [Key]
        public int JudgeId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        public int CourtId { get; set; }

        [StringLength(100)]
        public string? Chambers { get; set; }

        [StringLength(100)]
        public string? Courtroom { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [StringLength(1000)]
        public string? Biography { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [ForeignKey("CourtId")]
        public virtual Court Court { get; set; } = null!;
    }
}
