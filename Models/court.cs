using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a court in the system
    /// </summary>
    public class Court
    {
        [Key]
        public int CourtId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Type { get; set; } // District, Supreme, High Court, etc.

        [StringLength(50)]
        public string? Level { get; set; } // e.g., High, District, Magistrate

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
        public virtual ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
    }
}