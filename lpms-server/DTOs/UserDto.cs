using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? EmployeeId { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<UserRoleDto> UserRoles { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? EmployeeId { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UserRoleDto
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public RoleDto? Role { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public int RoleId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    public class RoleDto
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class CreateRoleDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<int> PermissionIds { get; set; } = new();
    }

    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Module { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;
    }
}
