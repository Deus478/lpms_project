using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing users and roles in the LCMS system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(LegalCaseDbContext context, IMapper mapper, ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();

            return Ok(_mapper.Map<List<UserDto>>(users));
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return Ok(_mapper.Map<UserDto>(user));
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user)
                .Collection(u => u.UserRoles)
                .Query()
                .Include(ur => ur.Role)
                .LoadAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, _mapper.Map<UserDto>(user));
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        [HttpPost("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserRoleDto>> AssignRole(int userId, AssignRoleDto assignRoleDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var role = await _context.Roles.FindAsync(assignRoleDto.RoleId);
            if (role == null)
                return BadRequest("Role not found");

            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == assignRoleDto.RoleId);

            if (existingUserRole != null)
                return BadRequest("User already has this role");

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = assignRoleDto.RoleId,
                ExpiresAt = assignRoleDto.ExpiresAt
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            await _context.Entry(userRole)
                .Reference(ur => ur.User)
                .LoadAsync();
            await _context.Entry(userRole)
                .Reference(ur => ur.Role)
                .LoadAsync();

            return CreatedAtAction(nameof(GetUser), new { id = userId }, _mapper.Map<UserRoleDto>(userRole));
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            return Ok(_mapper.Map<List<RoleDto>>(roles));
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();
            return Ok(_mapper.Map<List<PermissionDto>>(permissions));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
