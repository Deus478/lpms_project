using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for lawyer information
    /// </summary>
    public class LawyerDto
    {
        public int LawyerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BarNumber { get; set; }
        public string? Specialization { get; set; }
    }

    /// <summary>
    /// DTO for creating a new lawyer
    /// </summary>
    public class CreateLawyerDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "Bar number cannot exceed 50 characters")]
        public string? BarNumber { get; set; }

        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }
    }
}