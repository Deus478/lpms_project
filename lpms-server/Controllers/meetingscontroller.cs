using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace LegalCaseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MeetingsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(LegalCaseDbContext context, ILogger<MeetingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Add a minute entry to a meeting
        /// </summary>
        /// <param name="id">Meeting ID</param>
        /// <param name="dto">Minute content and optional DocumentId</param>
        [HttpPost("{id}/minutes")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MinuteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CreateMinuteDto), typeof(lpms_server.Swagger.Examples.CreateMinuteDtoExample))]
        public async Task<ActionResult<MinuteDto>> AddMinute(int id, CreateMinuteDto dto)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound();

            var minute = new Minute
            {
                MeetingId = id,
                Content = dto.Content,
                DocumentId = dto.DocumentId,
                RecordedDate = DateTime.UtcNow
            };
            _context.Minutes.Add(minute);
            await _context.SaveChangesAsync();

            return Ok(new MinuteDto
            {
                MinuteId = minute.MinuteId,
                MeetingId = id,
                RecordedDate = minute.RecordedDate,
                Content = minute.Content,
                DocumentId = minute.DocumentId
            });
        }

        /// <summary>
        /// List minutes for a meeting
        /// </summary>
        /// <param name="id">Meeting ID</param>
        [HttpGet("{id}/minutes")]
        [ProducesResponseType(typeof(List<MinuteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MinuteDto>>> GetMinutes(int id)
        {
            var exists = await _context.Meetings.AnyAsync(m => m.MeetingId == id);
            if (!exists) return NotFound();

            var minutes = await _context.Minutes
                .Where(m => m.MeetingId == id)
                .OrderBy(m => m.RecordedDate)
                .ToListAsync();
            var result = minutes.Select(m => new MinuteDto
            {
                MinuteId = m.MinuteId,
                MeetingId = m.MeetingId,
                RecordedDate = m.RecordedDate,
                Content = m.Content,
                DocumentId = m.DocumentId
            }).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Attach or change the linked document for an existing minute
        /// </summary>
        /// <param name="meetingId">Meeting ID</param>
        /// <param name="minuteId">Minute ID</param>
        /// <param name="dto">New DocumentId or null to clear</param>
        [HttpPatch("{meetingId}/minutes/{minuteId}/document")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMinuteDocument(int meetingId, int minuteId, [FromBody] UpdateMinuteDocumentDto dto)
        {
            var minute = await _context.Minutes.FirstOrDefaultAsync(m => m.MeetingId == meetingId && m.MinuteId == minuteId);
            if (minute == null) return NotFound();

            minute.DocumentId = dto.DocumentId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Alias using POST to attach or change the linked document for an existing minute
        /// </summary>
        [HttpPost("{meetingId}/minutes/{minuteId}/document")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetMinuteDocument(int meetingId, int minuteId, [FromBody] UpdateMinuteDocumentDto dto)
        {
            var minute = await _context.Minutes.FirstOrDefaultAsync(m => m.MeetingId == meetingId && m.MinuteId == minuteId);
            if (minute == null) return NotFound();

            minute.DocumentId = dto.DocumentId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Add a resolution to a meeting
        /// </summary>
        /// <param name="id">Meeting ID</param>
        /// <param name="dto">Resolution details</param>
        [HttpPost("{id}/resolutions")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ResolutionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(CreateResolutionDto), typeof(lpms_server.Swagger.Examples.CreateResolutionDtoExample))]
        public async Task<ActionResult<ResolutionDto>> AddResolution(int id, CreateResolutionDto dto)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound();

            var res = new Resolution
            {
                MeetingId = id,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                ResponsibleUserId = dto.ResponsibleUserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Resolutions.Add(res);
            await _context.SaveChangesAsync();

            return Ok(new ResolutionDto
            {
                ResolutionId = res.ResolutionId,
                MeetingId = id,
                Title = res.Title,
                Description = res.Description,
                Status = res.Status,
                DueDate = res.DueDate,
                ResponsibleUserId = res.ResponsibleUserId,
                CreatedAt = res.CreatedAt,
                CompletedAt = res.CompletedAt
            });
        }

        /// <summary>
        /// List resolutions for a meeting
        /// </summary>
        /// <param name="id">Meeting ID</param>
        [HttpGet("{id}/resolutions")]
        [ProducesResponseType(typeof(List<ResolutionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ResolutionDto>>> GetResolutions(int id)
        {
            var exists = await _context.Meetings.AnyAsync(m => m.MeetingId == id);
            if (!exists) return NotFound();

            var list = await _context.Resolutions
                .Where(r => r.MeetingId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
            var result = list.Select(r => new ResolutionDto
            {
                ResolutionId = r.ResolutionId,
                MeetingId = r.MeetingId,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                DueDate = r.DueDate,
                ResponsibleUserId = r.ResponsibleUserId,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            }).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Update resolution status (Pending, InProgress, Completed)
        /// </summary>
        /// <param name="meetingId">Meeting ID</param>
        /// <param name="resolutionId">Resolution ID</param>
        /// <param name="dto">Status update</param>
        [HttpPatch("{meetingId}/resolutions/{resolutionId}/status")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateResolutionStatusDto), typeof(lpms_server.Swagger.Examples.UpdateResolutionStatusDtoExample))]
        public async Task<IActionResult> UpdateResolutionStatus(int meetingId, int resolutionId, UpdateResolutionStatusDto dto)
        {
            var res = await _context.Resolutions.FirstOrDefaultAsync(r => r.MeetingId == meetingId && r.ResolutionId == resolutionId);
            if (res == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Status)) res.Status = dto.Status;
            res.CompletedAt = dto.CompletedAt;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Schedule a new meeting for a Board or Committee
        /// </summary>
        /// <remarks>Provide either BoardId or CommitteeId.</remarks>
        /// <param name="dto">Meeting details</param>
        /// <returns>Created meeting summary</returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(CreateMeetingDto), typeof(lpms_server.Swagger.Examples.CreateMeetingDtoExample))]
        public async Task<ActionResult<MeetingSummaryDto>> CreateMeeting(CreateMeetingDto dto)
        {
            if (dto.BoardId == null && dto.CommitteeId == null)
            {
                return BadRequest("Either BoardId or CommitteeId must be provided");
            }

            if (dto.BoardId.HasValue)
            {
                var exists = await _context.Boards.AnyAsync(b => b.BoardId == dto.BoardId.Value && b.IsActive);
                if (!exists) return BadRequest("Invalid BoardId");
            }
            if (dto.CommitteeId.HasValue)
            {
                var exists = await _context.Committees.AnyAsync(c => c.CommitteeId == dto.CommitteeId.Value && c.IsActive);
                if (!exists) return BadRequest("Invalid CommitteeId");
            }

            var meeting = new Meeting
            {
                BoardId = dto.BoardId,
                CommitteeId = dto.CommitteeId,
                ScheduledDate = dto.ScheduledDate,
                Location = dto.Location,
                Title = dto.Title,
                Agenda = dto.Agenda,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            return Ok(new MeetingSummaryDto
            {
                MeetingId = meeting.MeetingId,
                BoardId = meeting.BoardId,
                CommitteeId = meeting.CommitteeId,
                ScheduledDate = meeting.ScheduledDate,
                Title = meeting.Title,
                Status = meeting.Status,
                Location = meeting.Location,
                AttendanceCount = 0
            });
        }

        /// <summary>
        /// List meetings
        /// </summary>
        /// <returns>List of meeting summaries</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MeetingSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MeetingSummaryDto>>> GetMeetings()
        {
            var meetings = await _context.Meetings
                .Include(m => m.Attendances)
                .OrderByDescending(m => m.ScheduledDate)
                .ToListAsync();
            // Auto-update statuses based on schedule
            var now = DateTime.UtcNow;
            var changed = false;
            foreach (var m in meetings)
            {
                if (m.Status == "Scheduled" && m.ScheduledDate <= now)
                {
                    m.Status = "Completed";
                    m.UpdatedAt = DateTime.UtcNow;
                    changed = true;
                }
            }
            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            var result = meetings.Select(m => new MeetingSummaryDto
            {
                MeetingId = m.MeetingId,
                BoardId = m.BoardId,
                CommitteeId = m.CommitteeId,
                ScheduledDate = m.ScheduledDate,
                Title = m.Title,
                Status = m.Status,
                Location = m.Location,
                AttendanceCount = m.Attendances.Count
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get upcoming meetings
        /// </summary>
        /// <returns>List of upcoming meeting summaries</returns>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(List<MeetingSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MeetingSummaryDto>>> GetUpcomingMeetings()
        {
            var now = DateTime.UtcNow;
            var meetings = await _context.Meetings
                .Include(m => m.Attendances)
                .Where(m => m.ScheduledDate > now && m.Status == "Scheduled")
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();

            var result = meetings.Select(m => new MeetingSummaryDto
            {
                MeetingId = m.MeetingId,
                BoardId = m.BoardId,
                CommitteeId = m.CommitteeId,
                ScheduledDate = m.ScheduledDate,
                Title = m.Title,
                Status = m.Status,
                Location = m.Location,
                AttendanceCount = m.Attendances.Count
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Update a meeting's schedule, details, or status
        /// </summary>
        /// <param name="id">Meeting ID</param>
        /// <param name="dto">Fields to update</param>
        [HttpPatch("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMeeting(int id, UpdateMeetingDto dto)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound();

            if (dto.ScheduledDate.HasValue) meeting.ScheduledDate = dto.ScheduledDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.Location)) meeting.Location = dto.Location;
            if (!string.IsNullOrWhiteSpace(dto.Title)) meeting.Title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Agenda)) meeting.Agenda = dto.Agenda;
            if (!string.IsNullOrWhiteSpace(dto.Status)) meeting.Status = dto.Status;
            meeting.UpdatedAt = DateTime.UtcNow;

            // Ensure automatic completion if date has passed
            if (meeting.Status == "Scheduled" && meeting.ScheduledDate <= DateTime.UtcNow)
            {
                meeting.Status = "Completed";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Record attendance for a meeting
        /// </summary>
        /// <param name="id">Meeting ID</param>
        /// <param name="dto">Attendance list</param>
        [HttpPost("{id}/attendance")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(RecordAttendanceDto), typeof(lpms_server.Swagger.Examples.RecordAttendanceDtoExample))]
        public async Task<IActionResult> RecordAttendance(int id, RecordAttendanceDto dto)
        {
            var meeting = await _context.Meetings.Include(m => m.Attendances).FirstOrDefaultAsync(m => m.MeetingId == id);
            if (meeting == null) return NotFound();

            foreach (var item in dto.Items)
            {
                meeting.Attendances.Add(new MeetingAttendance
                {
                    MeetingId = id,
                    AttendeeName = item.AttendeeName,
                    AttendeeRole = item.AttendeeRole,
                    Present = item.Present,
                    Notes = item.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("recurring")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(RecurringSeriesResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecurringSeriesResultDto>> CreateRecurringMeetings(CreateRecurringMeetingDto dto)
        {
            if (dto.BoardId == null && dto.CommitteeId == null)
            {
                return BadRequest("Either BoardId or CommitteeId must be provided");
            }

            if (dto.BoardId.HasValue)
            {
                var exists = await _context.Boards.AnyAsync(b => b.BoardId == dto.BoardId.Value && b.IsActive);
                if (!exists) return BadRequest("Invalid BoardId");
            }
            if (dto.CommitteeId.HasValue)
            {
                var exists = await _context.Committees.AnyAsync(c => c.CommitteeId == dto.CommitteeId.Value && c.IsActive);
                if (!exists) return BadRequest("Invalid CommitteeId");
            }

            if ((dto.EndDate == null || dto.EndDate <= dto.StartDate) && (dto.Occurrences == null || dto.Occurrences <= 0))
            {
                return BadRequest("Provide a valid EndDate after StartDate or a positive Occurrences count");
            }
            if (dto.IntervalWeeks <= 0) dto.IntervalWeeks = 1;
            if (dto.DaysOfWeek == null || dto.DaysOfWeek.Count == 0)
            {
                return BadRequest("DaysOfWeek is required");
            }
            if (dto.DaysOfWeek.Any(d => d < 0 || d > 6))
            {
                return BadRequest("DaysOfWeek values must be between 0 (Sunday) and 6 (Saturday)");
            }

            var seriesId = Guid.NewGuid();
            var created = new List<Meeting>();

            var currentWeekStart = dto.StartDate.Date;
            int createdCount = 0;
            DateTime? endDate = dto.EndDate?.Date;

            while (true)
            {
                foreach (var dow in dto.DaysOfWeek.OrderBy(d => d))
                {
                    var occurrenceDate = currentWeekStart.AddDays(dow);
                    if (occurrenceDate < dto.StartDate.Date) continue;

                    if (endDate.HasValue && occurrenceDate > endDate.Value)
                    {
                        goto BuildResponse;
                    }

                    if (dto.Occurrences.HasValue && createdCount >= dto.Occurrences.Value)
                    {
                        goto BuildResponse;
                    }

                    var m = new Meeting
                    {
                        BoardId = dto.BoardId,
                        CommitteeId = dto.CommitteeId,
                        ScheduledDate = occurrenceDate,
                        Location = dto.Location,
                        Title = dto.Title,
                        Agenda = dto.Agenda,
                        Status = "Scheduled",
                        CreatedAt = DateTime.UtcNow,
                        SeriesId = seriesId,
                        RecurrenceType = "Weekly",
                        RecurrenceInterval = dto.IntervalWeeks,
                        RecurrenceDays = string.Join(',', dto.DaysOfWeek.OrderBy(d => d)),
                        RecurrenceEndDate = endDate
                    };
                    created.Add(m);
                    createdCount++;
                    if (dto.Occurrences.HasValue && createdCount >= dto.Occurrences.Value)
                    {
                        goto BuildResponse;
                    }
                }

                currentWeekStart = currentWeekStart.AddDays(7 * dto.IntervalWeeks);
                if (endDate.HasValue && currentWeekStart > endDate.Value)
                {
                    break;
                }
            }

        BuildResponse:
            if (created.Count == 0)
            {
                return BadRequest("No meetings generated with the provided parameters");
            }

            _context.Meetings.AddRange(created);
            await _context.SaveChangesAsync();

            var result = new RecurringSeriesResultDto
            {
                SeriesId = seriesId,
                CreatedMeetings = created
                    .OrderBy(m => m.ScheduledDate)
                    .Select(m => new MeetingSummaryDto
                    {
                        MeetingId = m.MeetingId,
                        BoardId = m.BoardId,
                        CommitteeId = m.CommitteeId,
                        ScheduledDate = m.ScheduledDate,
                        Title = m.Title,
                        Status = m.Status,
                        Location = m.Location,
                        AttendanceCount = 0
                    }).ToList()
            };

            return Ok(result);
        }

        [HttpGet("series/{seriesId}")]
        [ProducesResponseType(typeof(List<MeetingSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MeetingSummaryDto>>> GetMeetingsBySeries(Guid seriesId)
        {
            var list = await _context.Meetings
                .Where(m => m.SeriesId == seriesId)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();

            if (list.Count == 0)
            {
                return NotFound();
            }

            var result = list.Select(m => new MeetingSummaryDto
            {
                MeetingId = m.MeetingId,
                BoardId = m.BoardId,
                CommitteeId = m.CommitteeId,
                ScheduledDate = m.ScheduledDate,
                Title = m.Title,
                Status = m.Status,
                Location = m.Location,
                AttendanceCount = 0
            }).ToList();

            return Ok(result);
        }
    }
}


