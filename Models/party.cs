using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents a party involved in legal cases (plaintiff, defendant, etc.)
    /// </summary>
    public class Party
    {
        [Key]
        public int PartyId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PartyType { get; set; } // Individual, Corporation, Government, etc.

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";

        // Navigation Properties
        public virtual ICollection<CaseParty> CaseParties { get; set; } = new List<CaseParty>();
    }
}