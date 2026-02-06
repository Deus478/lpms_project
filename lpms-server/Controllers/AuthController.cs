using LegalCaseManagement.Data;
using LegalCaseManagement.DTOs;
using LegalCaseManagement.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalCaseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly LegalCaseDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(LegalCaseDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required");
            }

            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null)
            {
                return Unauthorized();
            }

            var roles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var token = _tokenService.CreateToken(user, roles);

            return Ok(new LoginResponseDto
            {
                Token = token,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles
            });
        }
    }
}
