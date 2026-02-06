using System.Security.Claims;
using System.Text.Json;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LegalCaseManagement.Infrastructure.Audit
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly LegalCaseDbContext _db;
        private readonly ILogger<AuditActionFilter> _logger;

        public AuditActionFilter(LegalCaseDbContext db, ILogger<AuditActionFilter> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            try
            {
                var http = context.HttpContext;
                var userIdStr = http.User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value
                                ?? http.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    return;
                }

                var action = http.Request.Method;
                var entityType = context.RouteData.Values.TryGetValue("controller", out var c) ? c?.ToString() ?? "" : "";
                var entityId = (int?)null;

                if (context.RouteData.Values.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out var parsedId))
                {
                    entityId = parsedId;
                }

                var ip = http.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
                var userAgent = http.Request.Headers.UserAgent.ToString();

                var newValues = (string?)null;
                if (context.ActionArguments.Count > 0)
                {
                    newValues = JsonSerializer.Serialize(context.ActionArguments);
                }

                _db.AuditTrails.Add(new AuditTrail
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityIdentifier = entityId?.ToString(),
                    Description = $"{entityType}.{context.ActionDescriptor.DisplayName}",
                    OldValues = null,
                    NewValues = newValues,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    IsSensitive = false
                });

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Audit logging failed");
            }
        }
    }
}
