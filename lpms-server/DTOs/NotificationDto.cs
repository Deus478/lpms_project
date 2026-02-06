using System.ComponentModel.DataAnnotations;

namespace LegalCaseManagement.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    public class CreateNotificationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [StringLength(100)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    public class NotificationPreferenceDto
    {
        public int NotificationPreferenceId { get; set; }
        public int UserId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateNotificationPreferenceDto
    {
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        public bool EmailEnabled { get; set; }

        public bool InAppEnabled { get; set; }

        public bool SmsEnabled { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public int CriticalCount { get; set; }
        public int HighPriorityCount { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new();
    }

    public class BulkNotificationDto
    {
        [Required]
        public List<int> UserIds { get; set; } = new();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [StringLength(100)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
