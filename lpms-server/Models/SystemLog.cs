using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents system logs for auditing and monitoring
    /// </summary>
    public class SystemLog
    {
        [Key]
        public int SystemLogId { get; set; }

        [Required]
        [StringLength(100)]
        public string LogLevel { get; set; } = string.Empty; // Info, Warning, Error, Critical

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Security, Performance, Business, System

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? Details { get; set; }

        [StringLength(100)]
        public string? Source { get; set; }

        [StringLength(100)]
        public string? UserId { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(100)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
