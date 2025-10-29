using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing courts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CourtsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CourtsController> _logger;

        public CourtsController(LegalCaseDbContext context, IMapper mapper, ILogger<CourtsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all active courts
        /// </summary>
        /// <returns>List of courts</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourtDto>>> GetCourts()
        {
            try
            {
                var courts = await _context.Courts
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var courtDtos = _mapper.Map<List<CourtDto>>(courts);
                return Ok(courtDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving courts");
                return StatusCode(500, "An error occurred while retrieving courts");
            }
        }

        /// <summary>
        /// Get court by ID
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns>Court details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourtDto>> GetCourt(int id)
        {
            try
            {
                var court = await _context.Courts
                    .FirstOrDefaultAsync(c => c.CourtId == id && c.IsActive);

                if (court == null)
                {
                    return NotFound($"Court with ID {id} not found");
                }

                var courtDto = _mapper.Map<CourtDto>(court);
                return Ok(courtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving court {CourtId}", id);
                return StatusCode(500, "An error occurred while retrieving the court");
            }
        }

        /// <summary>
        /// Create a new court
        /// </summary>
        /// <param name="createCourtDto">Court creation data</param>
        /// <returns>Created court details</returns>
        [HttpPost]
        public async Task<ActionResult<CourtDto>> CreateCourt(CreateCourtDto createCourtDto)
        {
            try
            {
                // Check if court name already exists in the same city/state
                var existingCourt = await _context.Courts
                    .FirstOrDefaultAsync(c => c.Name == createCourtDto.Name && 
                                            c.City == createCourtDto.City && 
                                            c.State == createCourtDto.State);
                if (existingCourt != null)
                {
                    return BadRequest("A court with this name already exists in the same city/state");
                }

                var court = _mapper.Map<Court>(createCourtDto);
                court.CreatedAt = DateTime.UtcNow;

                _context.Courts.Add(court);
                await _context.SaveChangesAsync();

                var courtDto = _mapper.Map<CourtDto>(court);
                return CreatedAtAction(nameof(GetCourt), new { id = court.CourtId }, courtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating court");
                return StatusCode(500, "An error occurred while creating the court");
            }
        }

        /// <summary>
        /// Update court information
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <param name="updateCourtDto">Updated court data</param>
        /// <returns>Updated court details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CourtDto>> UpdateCourt(int id, CreateCourtDto updateCourtDto)
        {
            try
            {
                var court = await _context.Courts.FindAsync(id);
                if (court == null || !court.IsActive)
                {
                    return NotFound($"Court with ID {id} not found");
                }

                // Check if court name already exists in the same city/state (excluding current court)
                var existingCourt = await _context.Courts
                    .FirstOrDefaultAsync(c => c.Name == updateCourtDto.Name && 
                                            c.City == updateCourtDto.City && 
                                            c.State == updateCourtDto.State && 
                                            c.CourtId != id);
                if (existingCourt != null)
                {
                    return BadRequest("A court with this name already exists in the same city/state");
                }

                // Update fields
                court.Name = updateCourtDto.Name;
                court.Type = updateCourtDto.Type;
                court.Address = updateCourtDto.Address;
                court.City = updateCourtDto.City;
                court.State = updateCourtDto.State;
                court.ZipCode = updateCourtDto.ZipCode;
                court.Phone = updateCourtDto.Phone;

                await _context.SaveChangesAsync();

                var courtDto = _mapper.Map<CourtDto>(court);
                return Ok(courtDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating court {CourtId}", id);
                return StatusCode(500, "An error occurred while updating the court");
            }
        }

        /// <summary>
        /// Deactivate a court (soft delete)
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateCourt(int id)
        {
            try
            {
                var court = await _context.Courts.FindAsync(id);
                if (court == null)
                {
                    return NotFound($"Court with ID {id} not found");
                }

                if (!court.IsActive)
                {
                    return BadRequest("Court is already inactive");
                }

                // Check if court has active cases
                var activeCases = await _context.Cases
                    .CountAsync(c => c.CourtId == id && c.IsActive);
                if (activeCases > 0)
                {
                    return BadRequest($"Cannot deactivate court. It has {activeCases} active case(s)");
                }

                court.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Court successfully deactivated", courtId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating court {CourtId}", id);
                return StatusCode(500, "An error occurred while deactivating the court");
            }
        }
    }
}