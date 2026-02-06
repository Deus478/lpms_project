using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalCaseManagement.Models
{
    /// <summary>
    /// Represents user notification preferences
    /// </summary>
    public class NotificationPreference
    {
        [Key]
        public int NotificationPreferenceId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty; // Email, SMS, InApp, Push

        public bool EmailEnabled { get; set; } = true;

        public bool InAppEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        [StringLength(100)]
        public string? Setting { get; set; } // JSON for additional settings

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
