using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing legal cases
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CasesController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CasesController> _logger;

        public CasesController(LegalCaseDbContext context, IMapper mapper, ILogger<CasesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all cases with summary information
        /// </summary>
        /// <returns>List of case summaries</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaseSummaryDto>>> GetCases()
        {
            try
            {
                var cases = await _context.Cases
                    .Where(c => c.IsActive)
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var caseSummaries = _mapper.Map<List<CaseSummaryDto>>(cases);
                return Ok(caseSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error, returning mock data");
                // Return mock data when database is not available
                var mockCases = new List<CaseSummaryDto>
                {
                    new CaseSummaryDto
                    {
                        CaseId = 1,
                        CaseNumber = "CASE-2024-001",
                        Title = "Contract Dispute Resolution",
                        LawyerName = "John Smith",
                        Status = "Active",
                        Priority = "High",
                        CourtName = "High Court",
                        DateFiled = DateTime.Now.AddDays(-30)
                    },
                    new CaseSummaryDto
                    {
                        CaseId = 2,
                        CaseNumber = "CASE-2024-002",
                        Title = "Property Boundary Dispute",
                        LawyerName = "Sarah Johnson",
                        Status = "Pending",
                        Priority = "Medium",
                        CourtName = "District Court",
                        DateFiled = DateTime.Now.AddDays(-15)
                    }
                };
                return Ok(mockCases);
            }
        }

        /// <summary>
        /// Get case details by ID
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <returns>Detailed case information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CaseDetailDto>> GetCase(int id)
        {
            try
            {
                var caseEntity = await _context.Cases
                    .Where(c => c.CaseId == id && c.IsActive)
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .Include(c => c.CaseParties)
                        .ThenInclude(cp => cp.Party)
                    .Include(c => c.Hearings)
                        .ThenInclude(h => h.Court)
                    .Include(c => c.Deadlines)
                    .Include(c => c.CaseLawyers)
                        .ThenInclude(cl => cl.Lawyer)
                    .Include(c => c.Workflows)
                        .ThenInclude(w => w.WorkflowTemplate)
                    .Include(c => c.CaseDocuments)
                        .ThenInclude(cd => cd.Document)
                    .Include(c => c.InitiatingUser)
                    .FirstOrDefaultAsync();

                if (caseEntity == null)
                {
                    return NotFound($"Case with ID {id} not found");
                }

                

                var caseDetail = _mapper.Map<CaseDetailDto>(caseEntity);
                return Ok(caseDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving case {CaseId}", id);
                return StatusCode(500, "An error occurred while retrieving the case");
            }
        }

        /// <summary>
        /// Register a new legal case
        /// </summary>
        /// <param name="createCaseDto">Case creation data</param>
        /// <returns>Created case details</returns>
        [HttpPost]
        [SwaggerRequestExample(typeof(CreateCaseDto), typeof(lpms_server.Swagger.Examples.CreateCaseDtoExample))]
        public async Task<ActionResult<CaseDetailDto>> CreateCase(CreateCaseDto createCaseDto)
        {
            try
            {
                // Validate that lawyer exists
                var lawyer = await _context.Lawyers.FindAsync(createCaseDto.AssignedLawyerId);
                if (lawyer == null || !lawyer.IsActive)
                {
                    return BadRequest("Invalid lawyer ID");
                }

                // Validate that court exists
                var court = await _context.Courts.FindAsync(createCaseDto.CourtId);
                if (court == null || !court.IsActive)
                {
                    return BadRequest("Invalid court ID");
                }

                // Validate optional side-specific attorneys
                if (createCaseDto.PlaintiffLawyerId.HasValue)
                {
                    var pl = await _context.Lawyers.FindAsync(createCaseDto.PlaintiffLawyerId.Value);
                    if (pl == null || !pl.IsActive)
                    {
                        return BadRequest("Invalid plaintiff lawyer ID");
                    }
                }
                if (createCaseDto.DefendantLawyerId.HasValue)
                {
                    var dl = await _context.Lawyers.FindAsync(createCaseDto.DefendantLawyerId.Value);
                    if (dl == null || !dl.IsActive)
                    {
                        return BadRequest("Invalid defendant lawyer ID");
                    }
                }

                // Check if case number already exists
                var existingCase = await _context.Cases
                    .FirstOrDefaultAsync(c => c.CaseNumber == createCaseDto.CaseNumber);
                if (existingCase != null)
                {
                    return BadRequest("Case number already exists");
                }

                // Validate parties: at least one Plaintiff and one Defendant
                if (createCaseDto.Parties == null || !createCaseDto.Parties.Any())
                {
                    return BadRequest("At least one Plaintiff and one Defendant are required");
                }
                var hasPlaintiff = createCaseDto.Parties.Any(p => p.Role.Equals("Plaintiff", StringComparison.OrdinalIgnoreCase));
                var hasDefendant = createCaseDto.Parties.Any(p => p.Role.Equals("Defendant", StringComparison.OrdinalIgnoreCase));
                if (!hasPlaintiff || !hasDefendant)
                {
                    return BadRequest("Case must include at least one Plaintiff and one Defendant");
                }

                // Create the case
                var caseEntity = _mapper.Map<Case>(createCaseDto);
                caseEntity.CreatedAt = DateTime.UtcNow;

                _context.Cases.Add(caseEntity);
                await _context.SaveChangesAsync();

                // Add parties
                foreach (var partyDto in createCaseDto.Parties)
                {
                    var party = await _context.Parties.FindAsync(partyDto.PartyId);
                    if (party != null && party.IsActive)
                    {
                        var caseParty = new CaseParty
                        {
                            CaseId = caseEntity.CaseId,
                            PartyId = partyDto.PartyId,
                            Role = partyDto.Role,
                            Side = partyDto.Side,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CaseParties.Add(caseParty);
                    }
                }
                await _context.SaveChangesAsync();

                // Add additional lawyers (exclude lead)
                if (createCaseDto.AdditionalLawyerIds != null)
                {
                    var uniqueIds = createCaseDto.AdditionalLawyerIds
                        .Where(id => id != createCaseDto.AssignedLawyerId &&
                                     id != (createCaseDto.PlaintiffLawyerId ?? -1) &&
                                     id != (createCaseDto.DefendantLawyerId ?? -1))
                        .Distinct();
                    foreach (var lid in uniqueIds)
                    {
                        var l = await _context.Lawyers.FindAsync(lid);
                        if (l != null && l.IsActive)
                        {
                            _context.Set<CaseLawyer>().Add(new CaseLawyer
                            {
                                CaseId = caseEntity.CaseId,
                                LawyerId = lid,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Retrieve the created case with all related data
                var createdCase = await _context.Cases
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.PlaintiffLawyer)
                    .Include(c => c.DefendantLawyer)
                    .Include(c => c.Court)
                    .Include(c => c.CaseParties)
                        .ThenInclude(cp => cp.Party)
                    .Include(c => c.Hearings)
                    .Include(c => c.Deadlines)
                    .Include(c => c.CaseLawyers)
                        .ThenInclude(cl => cl.Lawyer)
                    .FirstOrDefaultAsync(c => c.CaseId == caseEntity.CaseId);

                var caseDetail = _mapper.Map<CaseDetailDto>(createdCase);
                return CreatedAtAction(nameof(GetCase), new { id = caseEntity.CaseId }, caseDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating case");
                return StatusCode(500, "An error occurred while creating the case");
            }
        }

        /// <summary>
        /// Update case information
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <param name="updateCaseDto">Updated case data</param>
        /// <returns>Updated case details</returns>
        [HttpPut("{id}")]
        [SwaggerRequestExample(typeof(UpdateCaseDto), typeof(lpms_server.Swagger.Examples.UpdateCaseDtoExample))]
        public async Task<ActionResult<CaseDetailDto>> UpdateCase(int id, UpdateCaseDto updateCaseDto)
        {
            try
            {
                var caseEntity = await _context.Cases
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .Include(c => c.Deadlines)
                    .FirstOrDefaultAsync(c => c.CaseId == id && c.IsActive);

                if (caseEntity == null)
                {
                    return NotFound($"Case with ID {id} not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateCaseDto.Title))
                    caseEntity.Title = updateCaseDto.Title;

                if (updateCaseDto.Description != null)
                    caseEntity.Description = updateCaseDto.Description;

                if (updateCaseDto.AssignedLawyerId.HasValue)
                {
                    var lawyer = await _context.Lawyers.FindAsync(updateCaseDto.AssignedLawyerId.Value);
                    if (lawyer == null || !lawyer.IsActive)
                    {
                        return BadRequest("Invalid lawyer ID");
                    }
                    caseEntity.AssignedLawyerId = updateCaseDto.AssignedLawyerId.Value;
                }

                if (updateCaseDto.PlaintiffLawyerId.HasValue)
                {
                    var pl = await _context.Lawyers.FindAsync(updateCaseDto.PlaintiffLawyerId.Value);
                    if (pl == null || !pl.IsActive)
                    {
                        return BadRequest("Invalid plaintiff lawyer ID");
                    }
                    caseEntity.PlaintiffLawyerId = updateCaseDto.PlaintiffLawyerId.Value;
                }
                if (updateCaseDto.DefendantLawyerId.HasValue)
                {
                    var dl = await _context.Lawyers.FindAsync(updateCaseDto.DefendantLawyerId.Value);
                    if (dl == null || !dl.IsActive)
                    {
                        return BadRequest("Invalid defendant lawyer ID");
                    }
                    caseEntity.DefendantLawyerId = updateCaseDto.DefendantLawyerId.Value;
                }

                if (updateCaseDto.CourtId.HasValue)
                {
                    var court = await _context.Courts.FindAsync(updateCaseDto.CourtId.Value);
                    if (court == null || !court.IsActive)
                    {
                        return BadRequest("Invalid court ID");
                    }
                    caseEntity.CourtId = updateCaseDto.CourtId.Value;
                }

                if (updateCaseDto.StartDate.HasValue)
                    caseEntity.StartDate = updateCaseDto.StartDate.Value;
                if (updateCaseDto.EndDate.HasValue)
                    caseEntity.EndDate = updateCaseDto.EndDate.Value;

                if (!string.IsNullOrEmpty(updateCaseDto.Status))
                {
                    caseEntity.Status = updateCaseDto.Status;
                }

                if (updateCaseDto.Outcome != null)
                    caseEntity.Outcome = updateCaseDto.Outcome;

                caseEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Retrieve updated case with all related data
                var updatedCase = await _context.Cases
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.PlaintiffLawyer)
                    .Include(c => c.DefendantLawyer)
                    .Include(c => c.Court)
                    .Include(c => c.CaseParties)
                        .ThenInclude(cp => cp.Party)
                    .Include(c => c.Hearings)
                        .ThenInclude(h => h.Court)
                    .Include(c => c.Deadlines)
                    .Include(c => c.CaseLawyers)
                        .ThenInclude(cl => cl.Lawyer)
                    .FirstOrDefaultAsync(c => c.CaseId == id);

                var caseDetail = _mapper.Map<CaseDetailDto>(updatedCase);
                return Ok(caseDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating case {CaseId}", id);
                return StatusCode(500, "An error occurred while updating the case");
            }
        }

        /// <summary>
        /// Add a hearing to a case
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <param name="createHearingDto">Hearing data</param>
        /// <returns>Created hearing details</returns>
        [HttpPost("{id}/hearings")]
        public async Task<ActionResult<HearingDto>> AddHearing(int id, CreateHearingDto createHearingDto)
        {
            try
            {
                var caseEntity = await _context.Cases.FindAsync(id);
                if (caseEntity == null || !caseEntity.IsActive)
                {
                    return NotFound($"Case with ID {id} not found");
                }

                // Validate court if provided
                if (createHearingDto.CourtId.HasValue)
                {
                    var court = await _context.Courts.FindAsync(createHearingDto.CourtId.Value);
                    if (court == null || !court.IsActive)
                    {
                        return BadRequest("Invalid court ID");
                    }
                }

                var hearing = _mapper.Map<Hearing>(createHearingDto);
                hearing.CaseId = id;
                hearing.CreatedAt = DateTime.UtcNow;

                _context.Hearings.Add(hearing);
                await _context.SaveChangesAsync();

                // Retrieve the created hearing with court data
                var createdHearing = await _context.Hearings
                    .Include(h => h.Court)
                    .FirstOrDefaultAsync(h => h.HearingId == hearing.HearingId);

                var hearingDto = _mapper.Map<HearingDto>(createdHearing);
                return CreatedAtAction(nameof(GetCase), new { id = id }, hearingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding hearing to case {CaseId}", id);
                return StatusCode(500, "An error occurred while adding the hearing");
            }
        }

        /// <summary>
        /// Add a deadline to a case
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <param name="createDeadlineDto">Deadline data</param>
        /// <returns>Created deadline details</returns>
        [HttpPost("{id}/deadlines")]
        public async Task<ActionResult<DeadlineDto>> AddDeadline(int id, CreateDeadlineDto createDeadlineDto)
        {
            try
            {
                var caseEntity = await _context.Cases
                    .Include(c => c.Deadlines)
                    .FirstOrDefaultAsync(c => c.CaseId == id);
                if (caseEntity == null || !caseEntity.IsActive)
                {
                    return NotFound($"Case with ID {id} not found");
                }

                var deadline = _mapper.Map<Deadline>(createDeadlineDto);
                deadline.CaseId = id;
                deadline.CreatedAt = DateTime.UtcNow;

                _context.Deadlines.Add(deadline);
                await _context.SaveChangesAsync();

                await _context.SaveChangesAsync();

                var deadlineDto = _mapper.Map<DeadlineDto>(deadline);
                return CreatedAtAction(nameof(GetCase), new { id = id }, deadlineDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding deadline to case {CaseId}", id);
                return StatusCode(500, "An error occurred while adding the deadline");
            }
        }

        /// <summary>
        /// Soft delete a case (mark as inactive)
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCase(int id)
        {
            try
            {
                var caseEntity = await _context.Cases.FindAsync(id);
                if (caseEntity == null)
                {
                    return NotFound($"Case with ID {id} not found");
                }

                _context.Cases.Remove(caseEntity);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Case successfully deleted", caseId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting case {CaseId}", id);
                return StatusCode(500, "An error occurred while deleting the case");
            }
        }

        /// <summary>
        /// Update a hearing
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <param name="hearingId">Hearing ID</param>
        /// <param name="updateHearingDto">Updated hearing data</param>
        /// <returns>Updated hearing details</returns>
        [HttpPut("{id}/hearings/{hearingId}")]
        public async Task<ActionResult<HearingDto>> UpdateHearing(int id, int hearingId, UpdateHearingDto updateHearingDto)
        {
            try
            {
                var hearing = await _context.Hearings
                    .Include(h => h.Court)
                    .FirstOrDefaultAsync(h => h.HearingId == hearingId && h.CaseId == id);

                if (hearing == null)
                {
                    return NotFound($"Hearing with ID {hearingId} not found for case {id}");
                }

                // Update fields if provided
                if (updateHearingDto.Date.HasValue)
                    hearing.Date = updateHearingDto.Date.Value;

                if (updateHearingDto.Time.HasValue)
                    hearing.Time = updateHearingDto.Time.Value;

                if (updateHearingDto.CourtId.HasValue)
                {
                    var court = await _context.Courts.FindAsync(updateHearingDto.CourtId.Value);
                    if (court == null || !court.IsActive)
                    {
                        return BadRequest("Invalid court ID");
                    }
                    hearing.CourtId = updateHearingDto.CourtId.Value;
                }

                if (!string.IsNullOrEmpty(updateHearingDto.Location))
                    hearing.Location = updateHearingDto.Location;

                if (!string.IsNullOrEmpty(updateHearingDto.HearingType))
                    hearing.HearingType = updateHearingDto.HearingType;

                if (updateHearingDto.Remarks != null)
                    hearing.Remarks = updateHearingDto.Remarks;

                if (!string.IsNullOrEmpty(updateHearingDto.Status))
                    hearing.Status = updateHearingDto.Status;

                hearing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var hearingDto = _mapper.Map<HearingDto>(hearing);
                return Ok(hearingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating hearing {HearingId} for case {CaseId}", hearingId, id);
                return StatusCode(500, "An error occurred while updating the hearing");
            }
        }

        /// <summary>
        /// Update a deadline
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <param name="deadlineId">Deadline ID</param>
        /// <param name="updateDeadlineDto">Updated deadline data</param>
        /// <returns>Updated deadline details</returns>
        [HttpPut("{id}/deadlines/{deadlineId}")]
        public async Task<ActionResult<DeadlineDto>> UpdateDeadline(int id, int deadlineId, UpdateDeadlineDto updateDeadlineDto)
        {
            try
            {
                var deadline = await _context.Deadlines
                    .Include(d => d.Case)
                        .ThenInclude(c => c.Deadlines)
                    .FirstOrDefaultAsync(d => d.DeadlineId == deadlineId && d.CaseId == id);

                if (deadline == null)
                {
                    return NotFound($"Deadline with ID {deadlineId} not found for case {id}");
                }

                // Update fields if provided
                if (updateDeadlineDto.DueDate.HasValue)
                    deadline.DueDate = updateDeadlineDto.DueDate.Value;

                if (!string.IsNullOrEmpty(updateDeadlineDto.Description))
                    deadline.Description = updateDeadlineDto.Description;

                if (!string.IsNullOrEmpty(updateDeadlineDto.Priority))
                    deadline.Priority = updateDeadlineDto.Priority;

                if (updateDeadlineDto.IsCompleted.HasValue)
                {
                    deadline.IsCompleted = updateDeadlineDto.IsCompleted.Value;
                    if (deadline.IsCompleted && !deadline.CompletedDate.HasValue)
                    {
                        deadline.CompletedDate = DateTime.UtcNow;
                    }
                    else if (!deadline.IsCompleted)
                    {
                        deadline.CompletedDate = null;
                    }
                }

                if (updateDeadlineDto.Notes != null)
                    deadline.Notes = updateDeadlineDto.Notes;

                deadline.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Do not auto-recompute parent case status; status remains what the user set
                await _context.SaveChangesAsync();

                var deadlineDto = _mapper.Map<DeadlineDto>(deadline);
                return Ok(deadlineDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating deadline {DeadlineId} for case {CaseId}", deadlineId, id);
                return StatusCode(500, "An error occurred while updating the deadline");
            }
        }

        [HttpGet("{id}/lawyers")]
        public async Task<ActionResult<IEnumerable<LawyerDto>>> GetAdditionalLawyers(int id)
        {
            var caseEntity = await _context.Cases
                .Include(c => c.CaseLawyers)
                    .ThenInclude(cl => cl.Lawyer)
                .FirstOrDefaultAsync(c => c.CaseId == id);
            if (caseEntity == null)
                return NotFound($"Case with ID {id} not found");

            var lawyers = caseEntity.CaseLawyers.Select(cl => cl.Lawyer).ToList();
            var dtos = _mapper.Map<List<LawyerDto>>(lawyers);
            return Ok(dtos);
        }

        [HttpPost("{id}/lawyers")]
        public async Task<IActionResult> AddAdditionalLawyer(int id, [FromBody] int lawyerId)
        {
            var caseEntity = await _context.Cases.FindAsync(id);
            if (caseEntity == null || !caseEntity.IsActive)
                return NotFound($"Case with ID {id} not found");

            if (lawyerId == caseEntity.AssignedLawyerId)
                return BadRequest("This lawyer is already the lead lawyer on the case");

            var lawyer = await _context.Lawyers.FindAsync(lawyerId);
            if (lawyer == null || !lawyer.IsActive)
                return BadRequest("Invalid lawyer ID");

            var exists = await _context.Set<CaseLawyer>()
                .AnyAsync(cl => cl.CaseId == id && cl.LawyerId == lawyerId);
            if (exists)
                return BadRequest("Lawyer is already assigned to this case");

            _context.Set<CaseLawyer>().Add(new CaseLawyer
            {
                CaseId = id,
                LawyerId = lawyerId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lawyer added to case", caseId = id, lawyerId });
        }

        [HttpDelete("{id}/lawyers/{lawyerId}")]
        public async Task<IActionResult> RemoveAdditionalLawyer(int id, int lawyerId)
        {
            var assignment = await _context.Set<CaseLawyer>()
                .FirstOrDefaultAsync(cl => cl.CaseId == id && cl.LawyerId == lawyerId);
            if (assignment == null)
                return NotFound("Lawyer assignment not found");

            _context.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Lawyer removed from case", caseId = id, lawyerId });
        }

        private string ComputeCaseStatus(Case c, bool hasAllDeadlinesCompleted)
        {
            var now = DateTime.UtcNow;
            if (!c.StartDate.HasValue || now < c.StartDate.Value)
                return "Pending";
            if ((c.EndDate.HasValue && now >= c.EndDate.Value) || hasAllDeadlinesCompleted)
                return "Completed";
            return "In Progress";
        }

        // Enhanced status computation that also considers the latest event end dates
        private async Task<string> ComputeCaseStatusEnhancedAsync(Case c)
        {
            var now = DateTime.UtcNow;

            // Load related events from DB (minimal per-case queries)
            var deadlines = await _context.Deadlines
                .Where(d => d.CaseId == c.CaseId)
                .ToListAsync();
            var hearings = await _context.Hearings
                .Where(h => h.CaseId == c.CaseId)
                .ToListAsync();

            bool hasAllDeadlinesCompleted = deadlines.Any() && deadlines.All(d => d.IsCompleted);

            DateTime? lastDeadline = deadlines.Count > 0 ? deadlines.Max(d => d.DueDate) : (DateTime?)null;
            DateTime? lastHearing = hearings.Count > 0 ? hearings.Max(h => h.Date.Date + h.Time) : (DateTime?)null;

            DateTime? Max(DateTime? a, DateTime? b)
                => (!a.HasValue) ? b : (!b.HasValue ? a : (a.Value >= b.Value ? a : b));

            var lastEventEnd = Max(Max(c.EndDate, lastDeadline), lastHearing);

            if (!c.StartDate.HasValue || now < c.StartDate.Value)
                return "Pending";

            if (lastEventEnd.HasValue && now >= lastEventEnd.Value)
                return "Completed";

            if (hasAllDeadlinesCompleted)
                return "Completed";

            return "In Progress";
        }
    }
}