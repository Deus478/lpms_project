using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing lawyers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LawyersController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<LawyersController> _logger;

        public LawyersController(LegalCaseDbContext context, IMapper mapper, ILogger<LawyersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all active lawyers
        /// </summary>
        /// <returns>List of lawyers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LawyerDto>>> GetLawyers()
        {
            try
            {
                var lawyers = await _context.Lawyers
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.LastName)
                    .ThenBy(l => l.FirstName)
                    .ToListAsync();

                var lawyerDtos = _mapper.Map<List<LawyerDto>>(lawyers);
                return Ok(lawyerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving lawyers");
                return StatusCode(500, "An error occurred while retrieving lawyers");
            }
        }

        /// <summary>
        /// Get lawyer by ID
        /// </summary>
        /// <param name="id">Lawyer ID</param>
        /// <returns>Lawyer details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LawyerDto>> GetLawyer(int id)
        {
            try
            {
                var lawyer = await _context.Lawyers
                    .FirstOrDefaultAsync(l => l.LawyerId == id && l.IsActive);

                if (lawyer == null)
                {
                    return NotFound($"Lawyer with ID {id} not found");
                }

                var lawyerDto = _mapper.Map<LawyerDto>(lawyer);
                return Ok(lawyerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving lawyer {LawyerId}", id);
                return StatusCode(500, "An error occurred while retrieving the lawyer");
            }
        }

        /// <summary>
        /// Create a new lawyer
        /// </summary>
        /// <param name="createLawyerDto">Lawyer creation data</param>
        /// <returns>Created lawyer details</returns>
        [HttpPost]
        public async Task<ActionResult<LawyerDto>> CreateLawyer(CreateLawyerDto createLawyerDto)
        {
            try
            {
                // Check if email already exists
                var existingLawyer = await _context.Lawyers
                    .FirstOrDefaultAsync(l => l.Email == createLawyerDto.Email);
                if (existingLawyer != null)
                {
                    return BadRequest("A lawyer with this email already exists");
                }

                // Check if bar number already exists (if provided)
                if (!string.IsNullOrEmpty(createLawyerDto.BarNumber))
                {
                    var existingBarNumber = await _context.Lawyers
                        .FirstOrDefaultAsync(l => l.BarNumber == createLawyerDto.BarNumber);
                    if (existingBarNumber != null)
                    {
                        return BadRequest("A lawyer with this bar number already exists");
                    }
                }

                var lawyer = _mapper.Map<Lawyer>(createLawyerDto);
                lawyer.CreatedAt = DateTime.UtcNow;

                _context.Lawyers.Add(lawyer);
                await _context.SaveChangesAsync();

                var lawyerDto = _mapper.Map<LawyerDto>(lawyer);
                return CreatedAtAction(nameof(GetLawyer), new { id = lawyer.LawyerId }, lawyerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating lawyer");
                return StatusCode(500, "An error occurred while creating the lawyer");
            }
        }

        /// <summary>
        /// Update lawyer information
        /// </summary>
        /// <param name="id">Lawyer ID</param>
        /// <param name="updateLawyerDto">Updated lawyer data</param>
        /// <returns>Updated lawyer details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<LawyerDto>> UpdateLawyer(int id, CreateLawyerDto updateLawyerDto)
        {
            try
            {
                var lawyer = await _context.Lawyers.FindAsync(id);
                if (lawyer == null || !lawyer.IsActive)
                {
                    return NotFound($"Lawyer with ID {id} not found");
                }

                // Check if email already exists (excluding current lawyer)
                var existingLawyer = await _context.Lawyers
                    .FirstOrDefaultAsync(l => l.Email == updateLawyerDto.Email && l.LawyerId != id);
                if (existingLawyer != null)
                {
                    return BadRequest("A lawyer with this email already exists");
                }

                // Check if bar number already exists (excluding current lawyer)
                if (!string.IsNullOrEmpty(updateLawyerDto.BarNumber))
                {
                    var existingBarNumber = await _context.Lawyers
                        .FirstOrDefaultAsync(l => l.BarNumber == updateLawyerDto.BarNumber && l.LawyerId != id);
                    if (existingBarNumber != null)
                    {
                        return BadRequest("A lawyer with this bar number already exists");
                    }
                }

                // Update fields
                lawyer.FirstName = updateLawyerDto.FirstName;
                lawyer.LastName = updateLawyerDto.LastName;
                lawyer.Email = updateLawyerDto.Email;
                lawyer.Phone = updateLawyerDto.Phone;
                lawyer.BarNumber = updateLawyerDto.BarNumber;
                lawyer.Specialization = updateLawyerDto.Specialization;

                await _context.SaveChangesAsync();

                var lawyerDto = _mapper.Map<LawyerDto>(lawyer);
                return Ok(lawyerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating lawyer {LawyerId}", id);
                return StatusCode(500, "An error occurred while updating the lawyer");
            }
        }

        /// <summary>
        /// Deactivate a lawyer (soft delete)
        /// </summary>
        /// <param name="id">Lawyer ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateLawyer(int id)
        {
            try
            {
                var lawyer = await _context.Lawyers.FindAsync(id);
                if (lawyer == null)
                {
                    return NotFound($"Lawyer with ID {id} not found");
                }

                if (!lawyer.IsActive)
                {
                    return BadRequest("Lawyer is already inactive");
                }

                // Check if lawyer has active cases
                var activeCases = await _context.Cases
                    .CountAsync(c => c.AssignedLawyerId == id && c.IsActive);
                if (activeCases > 0)
                {
                    return BadRequest($"Cannot deactivate lawyer. They have {activeCases} active case(s)");
                }

                lawyer.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Lawyer successfully deactivated", lawyerId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating lawyer {LawyerId}", id);
                return StatusCode(500, "An error occurred while deactivating the lawyer");
            }
        }
    }
}