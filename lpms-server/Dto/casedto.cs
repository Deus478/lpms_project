using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for creating a new case
    /// </summary>
    public class CreateCaseDto
    {
        [Required(ErrorMessage = "Case number is required")]
        [StringLength(50, ErrorMessage = "Case number cannot exceed 50 characters")]
        public string CaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Assigned lawyer ID is required")]
        public int AssignedLawyerId { get; set; }

        // Side-specific attorneys (optional)
        public int? PlaintiffLawyerId { get; set; }
        public int? DefendantLawyerId { get; set; }

        [Required(ErrorMessage = "Court ID is required")]
        public int CourtId { get; set; }

        [Required(ErrorMessage = "Date filed is required")]
        public DateTime DateFiled { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Pending";

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = "Medium";

        public List<CasePartyDto>? Parties { get; set; }
        public List<int>? AdditionalLawyerIds { get; set; }
    }

    /// <summary>
    /// DTO for updating a case
    /// </summary>
    public class UpdateCaseDto
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public int? AssignedLawyerId { get; set; }

        public int? PlaintiffLawyerId { get; set; }
        public int? DefendantLawyerId { get; set; }

        public int? CourtId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string? Priority { get; set; }

        [StringLength(500, ErrorMessage = "Outcome cannot exceed 500 characters")]
        public string? Outcome { get; set; }
    }

    /// <summary>
    /// DTO for case summary (used in lists)
    /// </summary>
    public class CaseSummaryDto
    {
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LawyerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string CourtName { get; set; } = string.Empty;
        public DateTime DateFiled { get; set; }
    }

    /// <summary>
    /// DTO for detailed case information
    /// </summary>
    public class CaseDetailDto
    {
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Outcome { get; set; }
        public DateTime DateFiled { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related entities
        public LawyerDto AssignedLawyer { get; set; } = null!;
        public LawyerDto? PlaintiffLawyer { get; set; }
        public LawyerDto? DefendantLawyer { get; set; }
        public CourtDto Court { get; set; } = null!;
        public List<CasePartyDetailDto> Parties { get; set; } = new();
        public List<HearingDto> Hearings { get; set; } = new();
        public List<DeadlineDto> Deadlines { get; set; } = new();
        public List<LawyerDto> AdditionalLawyers { get; set; } = new();
    }

    /// <summary>
    /// DTO for case party relationship
    /// </summary>
    public class CasePartyDto
    {
        [Required(ErrorMessage = "Party ID is required")]
        public int PartyId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty; // Plaintiff, Defendant, etc.

        [Required(ErrorMessage = "Side is required")] 
        [StringLength(20, ErrorMessage = "Side cannot exceed 20 characters")]
        public string Side { get; set; } = "Accuser"; // Accuser or Accused
    }

    /// <summary>
    /// DTO for detailed case party information
    /// </summary>
    public class CasePartyDetailDto
    {
        public int CasePartyId { get; set; }
        public string Role { get; set; } = string.Empty;
        public PartyDto Party { get; set; } = null!;
    }
}