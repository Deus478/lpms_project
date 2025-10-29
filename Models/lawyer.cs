using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a lawyer in the system
    /// </summary>
    public class Lawyer
    {
        [Key]
        public int LawyerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(50)]
        public string? BarNumber { get; set; }

        [StringLength(100)]
        public string? Specialization { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";

        // Navigation Properties
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}