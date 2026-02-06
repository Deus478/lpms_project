using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LegalCaseManagement.Data;
using LegalCaseManagement.DTOs;
using LegalCaseManagement.Models;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for generating case reports and analytics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(LegalCaseDbContext context, IMapper mapper, ILogger<ReportsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get resolution progress summary (counts by status, overdue)
        /// </summary>
        [HttpGet("resolutions/summary")]
        public async Task<ActionResult<object>> GetResolutionSummary()
        {
            var now = DateTime.UtcNow.Date;
            var q = _context.Resolutions.AsQueryable();
            var total = await q.CountAsync();
            var pending = await q.CountAsync(r => r.Status == "Pending");
            var inProgress = await q.CountAsync(r => r.Status == "InProgress");
            var completed = await q.CountAsync(r => r.Status == "Completed");
            var overdue = await q.CountAsync(r => (r.Status == "Pending" || r.Status == "InProgress") && r.DueDate != null && r.DueDate < now);

            return Ok(new { total, pending, inProgress, completed, overdue });
        }

        /// <summary>
        /// Get resolutions by meeting
        /// </summary>
        [HttpGet("resolutions/by-meeting/{meetingId}")]
        public async Task<ActionResult<List<ResolutionDto>>> GetResolutionsByMeeting(int meetingId)
        {
            var list = await _context.Resolutions.Where(r => r.MeetingId == meetingId).ToListAsync();
            var result = list.Select(r => new ResolutionDto
            {
                ResolutionId = r.ResolutionId,
                MeetingId = r.MeetingId,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                DueDate = r.DueDate,
                ResponsibleUserId = r.ResponsibleUserId,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            }).ToList();
            return Ok(result);
        }
        /// <summary>
        /// Generate comprehensive case report
        /// </summary>
        /// <returns>Complete case report with statistics</returns>
        /// <summary>
        /// Generate comprehensive case report
        /// </summary>
        /// <returns>Complete case report with statistics</returns>
        [HttpGet]
        public async Task<ActionResult<CaseReportDto>> GetCaseReport()
        {
            try
            {
                var cases = await _context.Cases
                    .Where(c => c.IsActive)
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .Include(c => c.Hearings)
                    .Include(c => c.Deadlines)
                    .ToListAsync();

                var lawyers = await _context.Lawyers
                    .Where(l => l.IsActive)
                    .Include(l => l.Cases.Where(c => c.IsActive))
                        .ThenInclude(c => c.Hearings)
                    .Include(l => l.Cases.Where(c => c.IsActive))
                        .ThenInclude(c => c.Deadlines)
                    .ToListAsync();

                var courts = await _context.Courts
                    .Where(c => c.IsActive)
                    .Include(c => c.Cases.Where(ca => ca.IsActive))
                    .Include(c => c.Hearings.Where(h => h.Case.IsActive))
                    .ToListAsync();

                var report = new CaseReportDto
                {
                    TotalCases = cases.Count,
                    ActiveCases = cases.Count(c => c.Status == "Active"),
                    ClosedCases = cases.Count(c => c.Status == "Closed"),
                    PendingCases = cases.Count(c => c.Status == "Pending"),
                    Cases = _mapper.Map<List<CaseReportItemDto>>(cases),
                    LawyerCaseloads = _mapper.Map<List<LawyerCaseloadDto>>(lawyers),
                    CourtCaseloads = _mapper.Map<List<CourtCaseloadDto>>(courts),
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating case report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        /// <summary>
        /// Get deadline summary and upcoming deadlines
        /// </summary>
        /// <param name="days">Number of days to look ahead for upcoming deadlines (default: 30)</param>
        /// <returns>Deadline summary with upcoming deadlines</returns>
        [HttpGet("deadlines")]
        public async Task<ActionResult<DeadlineSummaryDto>> GetDeadlineSummary(int days = 30)
        {
            try
            {
                var allDeadlines = await _context.Deadlines
                    .Include(d => d.Case)
                        .ThenInclude(c => c.AssignedLawyer)
                    .Where(d => d.Case.IsActive)
                    .ToListAsync();

                var upcomingDeadlines = allDeadlines
                    .Where(d => !d.IsCompleted && d.DueDate > DateTime.UtcNow && d.DueDate <= DateTime.UtcNow.AddDays(days))
                    .OrderBy(d => d.DueDate)
                    .Select(d => new UpcomingDeadlineDto
                    {
                        DeadlineId = d.DeadlineId,
                        CaseNumber = d.Case.CaseNumber,
                        Description = d.Description,
                        DueDate = d.DueDate,
                        Priority = d.Priority,
                        DaysRemaining = (int)(d.DueDate - DateTime.UtcNow).TotalDays,
                        LawyerName = d.Case.AssignedLawyer.FullName
                    })
                    .ToList();

                var summary = new DeadlineSummaryDto
                {
                    TotalDeadlines = allDeadlines.Count,
                    CompletedDeadlines = allDeadlines.Count(d => d.IsCompleted),
                    PendingDeadlines = allDeadlines.Count(d => !d.IsCompleted && d.DueDate > DateTime.UtcNow),
                    OverdueDeadlines = allDeadlines.Count(d => !d.IsCompleted && d.DueDate < DateTime.UtcNow),
                    UpcomingDeadlines = upcomingDeadlines
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating deadline summary");
                return StatusCode(500, "An error occurred while generating the deadline summary");
            }
        }

        /// <summary>
        /// Get lawyer performance report
        /// </summary>
        /// <returns>Lawyer caseload and performance metrics</returns>
        /// <summary>
        /// Get lawyer performance report
        /// </summary>
        /// <returns>Lawyer caseload and performance metrics</returns>
        [HttpGet("lawyers")]
        public async Task<ActionResult<List<LawyerCaseloadDto>>> GetLawyerReport()
        {
            try
            {
                var lawyers = await _context.Lawyers
                    .Where(l => l.IsActive)
                    .Include(l => l.Cases.Where(c => c.IsActive))
                        .ThenInclude(c => c.Hearings)
                    .Include(l => l.Cases.Where(c => c.IsActive))
                        .ThenInclude(c => c.Deadlines)
                    .ToListAsync();

                var lawyerReport = _mapper.Map<List<LawyerCaseloadDto>>(lawyers);
                return Ok(lawyerReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating lawyer report");
                return StatusCode(500, "An error occurred while generating the lawyer report");
            }
        }

        /// <summary>
        /// Get court utilization report
        /// </summary>
        /// <returns>Court caseload and utilization metrics</returns>
        /// <summary>
        /// Get court utilization report
        /// </summary>
        /// <returns>Court caseload and utilization metrics</returns>
        [HttpGet("courts")]
        public async Task<ActionResult<List<CourtCaseloadDto>>> GetCourtReport()
        {
            try
            {
                var courts = await _context.Courts
                    .Where(c => c.IsActive)
                    .Include(c => c.Cases.Where(ca => ca.IsActive))
                    .Include(c => c.Hearings.Where(h => h.Case.IsActive))
                    .ToListAsync();

                var courtReport = _mapper.Map<List<CourtCaseloadDto>>(courts);
                return Ok(courtReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating court report");
                return StatusCode(500, "An error occurred while generating the court report");
            }
        }

        /// <summary>
        /// Get cases by status
        /// </summary>
        /// <param name="status">Case status to filter by</param>
        /// <returns>Cases with the specified status</returns>
        /// <summary>
        /// Get cases by status
        /// </summary>
        /// <param name="status">Case status to filter by</param>
        /// <returns>Cases with the specified status</returns>
        [HttpGet("cases/status/{status}")]
        public async Task<ActionResult<List<CaseSummaryDto>>> GetCasesByStatus(string status)
        {
            try
            {
                var cases = await _context.Cases
                    .Where(c => c.IsActive && c.Status.ToLower() == status.ToLower())
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var caseSummaries = _mapper.Map<List<CaseSummaryDto>>(cases);
                return Ok(caseSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cases by status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving cases by status");
            }
        }

        /// <summary>
        /// Get cases by lawyer
        /// </summary>
        /// <param name="lawyerId">Lawyer ID</param>
        /// <returns>Cases assigned to the specified lawyer</returns>
        [HttpGet("cases/lawyer/{lawyerId}")]
        public async Task<ActionResult<List<CaseSummaryDto>>> GetCasesByLawyer(int lawyerId)
        {
            try
            {
                var lawyer = await _context.Lawyers.FindAsync(lawyerId);
                if (lawyer == null || !lawyer.IsActive)
                {
                    return NotFound($"Lawyer with ID {lawyerId} not found");
                }

                var cases = await _context.Cases
                    .Where(c => c.IsActive && c.AssignedLawyerId == lawyerId)
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var caseSummaries = _mapper.Map<List<CaseSummaryDto>>(cases);
                return Ok(caseSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cases for lawyer {LawyerId}", lawyerId);
                return StatusCode(500, "An error occurred while retrieving cases for the lawyer");
            }
        }

        /// <summary>
        /// Get cases by court
        /// </summary>
        /// <param name="courtId">Court ID</param>
        /// <returns>Cases filed in the specified court</returns>
        [HttpGet("cases/court/{courtId}")]
        public async Task<ActionResult<List<CaseSummaryDto>>> GetCasesByCourt(int courtId)
        {
            try
            {
                var court = await _context.Courts.FindAsync(courtId);
                if (court == null || !court.IsActive)
                {
                    return NotFound($"Court with ID {courtId} not found");
                }

                var cases = await _context.Cases
                    .Where(c => c.IsActive && c.CourtId == courtId)
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var caseSummaries = _mapper.Map<List<CaseSummaryDto>>(cases);
                return Ok(caseSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cases for court {CourtId}", courtId);
                return StatusCode(500, "An error occurred while retrieving cases for the court");
            }
        }

        /// <summary>
        /// Get upcoming hearings
        /// </summary>
        /// <param name="days">Number of days to look ahead (default: 30)</param>
        /// <returns>Upcoming hearings within the specified timeframe</returns>
        [HttpGet("hearings/upcoming")]
        public async Task<ActionResult<List<HearingDto>>> GetUpcomingHearings(int days = 30)
        {
            try
            {
                var upcomingHearings = await _context.Hearings
                    .Where(h => h.Case.IsActive && 
                               h.Status == "Scheduled" && 
                               h.Date > DateTime.UtcNow && 
                               h.Date <= DateTime.UtcNow.AddDays(days))
                    .Include(h => h.Court)
                    .Include(h => h.Case)
                        .ThenInclude(c => c.AssignedLawyer)
                    .OrderBy(h => h.Date)
                    .ThenBy(h => h.Time)
                    .ToListAsync();

                var hearingDtos = _mapper.Map<List<HearingDto>>(upcomingHearings);
                return Ok(hearingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving upcoming hearings");
                return StatusCode(500, "An error occurred while retrieving upcoming hearings");
            }
        }

        /// <summary>
        /// Get overdue deadlines
        /// </summary>
        /// <returns>All overdue deadlines</returns>
        [HttpGet("deadlines/overdue")]
        public async Task<ActionResult<List<DeadlineDto>>> GetOverdueDeadlines()
        {
            try
            {
                var overdueDeadlines = await _context.Deadlines
                    .Where(d => d.Case.IsActive && 
                               !d.IsCompleted && 
                               d.DueDate < DateTime.UtcNow)
                    .Include(d => d.Case)
                        .ThenInclude(c => c.AssignedLawyer)
                    .OrderBy(d => d.DueDate)
                    .ToListAsync();

                var deadlineDtos = _mapper.Map<List<DeadlineDto>>(overdueDeadlines);
                return Ok(deadlineDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving overdue deadlines");
                return StatusCode(500, "An error occurred while retrieving overdue deadlines");
            }
        }
    }
}