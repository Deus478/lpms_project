using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for court information
    /// </summary>
    public class CourtDto
    {
        public int CourtId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Level { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>
    /// DTO for creating a new court
    /// </summary>
    public class CreateCourtDto
    {
        [Required(ErrorMessage = "Court name is required")]
        [StringLength(200, ErrorMessage = "Court name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string? Type { get; set; }

        [StringLength(50, ErrorMessage = "Level cannot exceed 50 characters")]
        public string? Level { get; set; }

        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
        public string? ZipCode { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }
    }
}