using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing parties
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PartiesController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PartiesController> _logger;

        public PartiesController(LegalCaseDbContext context, IMapper mapper, ILogger<PartiesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all active parties
        /// </summary>
        /// <returns>List of parties</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartyDto>>> GetParties()
        {
            try
            {
                var parties = await _context.Parties
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync();

                var partyDtos = _mapper.Map<List<PartyDto>>(parties);
                return Ok(partyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving parties");
                return StatusCode(500, "An error occurred while retrieving parties");
            }
        }

        /// <summary>
        /// Get party by ID
        /// </summary>
        /// <param name="id">Party ID</param>
        /// <returns>Party details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PartyDto>> GetParty(int id)
        {
            try
            {
                var party = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == id && p.IsActive);

                if (party == null)
                {
                    return NotFound($"Party with ID {id} not found");
                }

                var partyDto = _mapper.Map<PartyDto>(party);
                return Ok(partyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving party {PartyId}", id);
                return StatusCode(500, "An error occurred while retrieving the party");
            }
        }

        /// <summary>
        /// Create a new party
        /// </summary>
        /// <param name="createPartyDto">Party creation data</param>
        /// <returns>Created party details</returns>
        [HttpPost]
        public async Task<ActionResult<PartyDto>> CreateParty(CreatePartyDto createPartyDto)
        {
            try
            {
                // Check if email already exists (if provided)
                if (!string.IsNullOrEmpty(createPartyDto.Email))
                {
                    var existingParty = await _context.Parties
                        .FirstOrDefaultAsync(p => p.Email == createPartyDto.Email);
                    if (existingParty != null)
                    {
                        return BadRequest("A party with this email already exists");
                    }
                }

                var party = _mapper.Map<Party>(createPartyDto);
                party.CreatedAt = DateTime.UtcNow;

                _context.Parties.Add(party);
                await _context.SaveChangesAsync();

                var partyDto = _mapper.Map<PartyDto>(party);
                return CreatedAtAction(nameof(GetParty), new { id = party.PartyId }, partyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating party");
                return StatusCode(500, "An error occurred while creating the party");
            }
        }

        /// <summary>
        /// Update party information
        /// </summary>
        /// <param name="id">Party ID</param>
        /// <param name="updatePartyDto">Updated party data</param>
        /// <returns>Updated party details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<PartyDto>> UpdateParty(int id, CreatePartyDto updatePartyDto)
        {
            try
            {
                var party = await _context.Parties.FindAsync(id);
                if (party == null || !party.IsActive)
                {
                    return NotFound($"Party with ID {id} not found");
                }

                // Check if email already exists (excluding current party)
                if (!string.IsNullOrEmpty(updatePartyDto.Email))
                {
                    var existingParty = await _context.Parties
                        .FirstOrDefaultAsync(p => p.Email == updatePartyDto.Email && p.PartyId != id);
                    if (existingParty != null)
                    {
                        return BadRequest("A party with this email already exists");
                    }
                }

                // Update fields
                party.FirstName = updatePartyDto.FirstName;
                party.LastName = updatePartyDto.LastName;
                party.PartyType = updatePartyDto.PartyType;
                party.Email = updatePartyDto.Email;
                party.Phone = updatePartyDto.Phone;
                party.Address = updatePartyDto.Address;
                party.City = updatePartyDto.City;
                party.State = updatePartyDto.State;
                party.ZipCode = updatePartyDto.ZipCode;

                await _context.SaveChangesAsync();

                var partyDto = _mapper.Map<PartyDto>(party);
                return Ok(partyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating party {PartyId}", id);
                return StatusCode(500, "An error occurred while updating the party");
            }
        }

        /// <summary>
        /// Deactivate a party (soft delete)
        /// </summary>
        /// <param name="id">Party ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateParty(int id)
        {
            try
            {
                var party = await _context.Parties.FindAsync(id);
                if (party == null)
                {
                    return NotFound($"Party with ID {id} not found");
                }

                if (!party.IsActive)
                {
                    return BadRequest("Party is already inactive");
                }

                // Check if party is involved in active cases
                var activeCaseParties = await _context.CaseParties
                    .Include(cp => cp.Case)
                    .CountAsync(cp => cp.PartyId == id && cp.Case.IsActive);
                if (activeCaseParties > 0)
                {
                    return BadRequest($"Cannot deactivate party. They are involved in {activeCaseParties} active case(s)");
                }

                party.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Party successfully deactivated", partyId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating party {PartyId}", id);
                return StatusCode(500, "An error occurred while deactivating the party");
            }
        }

        /// <summary>
        /// Get cases for a specific party
        /// </summary>
        /// <param name="id">Party ID</param>
        /// <returns>List of cases involving the party</returns>
        [HttpGet("{id}/cases")]
        public async Task<ActionResult<IEnumerable<CaseSummaryDto>>> GetPartyCases(int id)
        {
            try
            {
                var party = await _context.Parties.FindAsync(id);
                if (party == null || !party.IsActive)
                {
                    return NotFound($"Party with ID {id} not found");
                }

                var cases = await _context.Cases
                    .Where(c => c.IsActive && c.CaseParties.Any(cp => cp.PartyId == id))
                    .Include(c => c.AssignedLawyer)
                    .Include(c => c.Court)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var caseSummaries = _mapper.Map<List<CaseSummaryDto>>(cases);
                return Ok(caseSummaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cases for party {PartyId}", id);
                return StatusCode(500, "An error occurred while retrieving cases for the party");
            }
        }
    }
}