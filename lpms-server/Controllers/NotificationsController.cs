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
    /// Controller for managing notifications in the LCMS system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(LegalCaseDbContext context, IMapper mapper, ILogger<NotificationsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get notifications for a user
        /// </summary>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(int userId, [FromQuery] bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<NotificationDto>>(notifications));
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("{id}/read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        [HttpPost("users/{userId}/read-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Create a notification
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto createDto)
        {
            var notification = _mapper.Map<Notification>(createDto);
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserNotifications), new { userId = notification.UserId }, _mapper.Map<NotificationDto>(notification));
        }

        /// <summary>
        /// Get notification preferences for a user
        /// </summary>
        [HttpGet("preferences/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationPreferenceDto>>> GetNotificationPreferences(int userId)
        {
            var preferences = await _context.NotificationPreferences
                .Where(np => np.UserId == userId)
                .ToListAsync();

            return Ok(_mapper.Map<List<NotificationPreferenceDto>>(preferences));
        }

        /// <summary>
        /// Update notification preferences
        /// </summary>
        [HttpPut("preferences/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateNotificationPreferences(int userId, List<UpdateNotificationPreferenceDto> preferences)
        {
            foreach (var prefDto in preferences)
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == prefDto.NotificationType);

                if (preference != null)
                {
                    preference.EmailEnabled = prefDto.EmailEnabled;
                    preference.InAppEnabled = prefDto.InAppEnabled;
                    preference.SmsEnabled = prefDto.SmsEnabled;
                    preference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    preference = new NotificationPreference
                    {
                        UserId = userId,
                        NotificationType = prefDto.NotificationType,
                        EmailEnabled = prefDto.EmailEnabled,
                        InAppEnabled = prefDto.InAppEnabled,
                        SmsEnabled = prefDto.SmsEnabled
                    };
                    _context.NotificationPreferences.Add(preference);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Get unread notification count for a user
        /// </summary>
        [HttpGet("users/{userId}/unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCount(int userId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(count);
        }

        /// <summary>
        /// Delete old notifications
        /// </summary>
        [HttpDelete("cleanup")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CleanupOldNotifications([FromQuery] int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
