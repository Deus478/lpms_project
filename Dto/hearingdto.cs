using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for hearing information
    /// </summary>
    public class HearingDto
    {
        public int HearingId { get; set; }
        public int CaseId { get; set; }
        public int? CourtId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Location { get; set; }
        public string? HearingType { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public CourtDto? Court { get; set; }
    }

    /// <summary>
    /// DTO for creating a new hearing
    /// </summary>
    public class CreateHearingDto
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required")]
        public TimeSpan Time { get; set; }

        public int? CourtId { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [StringLength(100, ErrorMessage = "Hearing type cannot exceed 100 characters")]
        public string? HearingType { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for updating a hearing
    /// </summary>
    public class UpdateHearingDto
    {
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public int? CourtId { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [StringLength(100, ErrorMessage = "Hearing type cannot exceed 100 characters")]
        public string? HearingType { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}