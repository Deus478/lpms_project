namespace LegalCaseManagement.DTOs
{
    /// <summary>
    /// DTO for case reports
    /// </summary>
    public class CaseReportDto
    {
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int ClosedCases { get; set; }
        public int PendingCases { get; set; }
        public List<CaseReportItemDto> Cases { get; set; } = new();
        public List<LawyerCaseloadDto> LawyerCaseloads { get; set; } = new();
        public List<CourtCaseloadDto> CourtCaseloads { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for individual case report item
    /// </summary>
    public class CaseReportItemDto
    {
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LawyerName { get; set; } = string.Empty;
        public string CourtName { get; set; } = string.Empty;
        public DateTime DateFiled { get; set; }
        public int TotalHearings { get; set; }
        public int CompletedHearings { get; set; }
        public int TotalDeadlines { get; set; }
        public int CompletedDeadlines { get; set; }
        public int OverdueDeadlines { get; set; }
        public DateTime? NextHearingDate { get; set; }
        public DateTime? NextDeadlineDate { get; set; }
    }

    /// <summary>
    /// DTO for lawyer caseload information
    /// </summary>
    public class LawyerCaseloadDto
    {
        public int LawyerId { get; set; }
        public string LawyerName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int ClosedCases { get; set; }
        public int UpcomingHearings { get; set; }
        public int OverdueDeadlines { get; set; }
    }

    /// <summary>
    /// DTO for court caseload information
    /// </summary>
    public class CourtCaseloadDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public string CourtType { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int UpcomingHearings { get; set; }
    }

    /// <summary>
    /// DTO for deadline summary
    /// </summary>
    public class DeadlineSummaryDto
    {
        public int TotalDeadlines { get; set; }
        public int CompletedDeadlines { get; set; }
        public int PendingDeadlines { get; set; }
        public int OverdueDeadlines { get; set; }
        public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();
    }

    /// <summary>
    /// DTO for upcoming deadline information
    /// </summary>
    public class UpcomingDeadlineDto
    {
        public int DeadlineId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
        public string LawyerName { get; set; } = string.Empty;
    }
}