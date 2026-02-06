using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LegalCaseManagement.Infrastructure.Notifications
{
    public class NotificationSchedulerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationSchedulerHostedService> _logger;
        private DateTime _lastSchemaWarningUtc = DateTime.MinValue;

        public NotificationSchedulerHostedService(IServiceProvider serviceProvider, ILogger<NotificationSchedulerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<LegalCaseDbContext>();

                    if (!await IsSchemaReadyAsync(db, stoppingToken))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        continue;
                    }

                    await GenerateContractRenewalNotifications(db, stoppingToken);
                    await GenerateOverdueResolutionNotifications(db, stoppingToken);
                    await GenerateRetentionExpiryNotifications(db, stoppingToken);
                    await GenerateUpcomingHearingNotifications(db, stoppingToken);
                }
                catch (ObjectDisposedException)
                {
                    // ServiceProvider disposed during host shutdown; exit gracefully
                    return;
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown requested
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Notification scheduler iteration failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (ObjectDisposedException)
                {
                    // Token or timer disposed as host is stopping
                    return;
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown during delay
                    return;
                }
            }
        }

        private async Task<bool> IsSchemaReadyAsync(LegalCaseDbContext db, CancellationToken ct)
        {
            var hasContracts = await TableExistsAsync(db, "Contracts", ct);
            var hasNotifications = await TableExistsAsync(db, "Notifications", ct);
            var hasDocuments = await TableExistsAsync(db, "Documents", ct);
            var hasHearings = await TableExistsAsync(db, "Hearings", ct);
            var hasResolutions = await TableExistsAsync(db, "Resolutions", ct);

            if (hasContracts && hasNotifications && hasDocuments && hasHearings && hasResolutions)
            {
                return true;
            }

            var now = DateTime.UtcNow;
            if (now - _lastSchemaWarningUtc >= TimeSpan.FromMinutes(5))
            {
                _lastSchemaWarningUtc = now;
                _logger.LogInformation("Database schema is not ready for notification scheduler (missing tables). Ensure migrations have been applied.");
            }

            return false;
        }

        private static async Task<bool> TableExistsAsync(LegalCaseDbContext db, string tableName, CancellationToken ct)
        {
            try
            {
                var conn = db.Database.GetDbConnection();
                var shouldClose = conn.State != ConnectionState.Open;
                if (shouldClose)
                {
                    await conn.OpenAsync(ct);
                }

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @name";
                var p = cmd.CreateParameter();
                p.ParameterName = "@name";
                p.Value = tableName;
                cmd.Parameters.Add(p);

                var result = await cmd.ExecuteScalarAsync(ct);

                if (shouldClose)
                {
                    await conn.CloseAsync();
                }

                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> ExistsRecentAsync(LegalCaseDbContext db, string type, string entityType, int? entityId, int userId, CancellationToken ct)
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            return await db.Notifications.AnyAsync(n =>
                n.Type == type &&
                n.EntityType == entityType &&
                n.EntityId == entityId &&
                n.UserId == userId &&
                n.CreatedAt >= cutoff,
                ct);
        }

        private static async Task GenerateContractRenewalNotifications(LegalCaseDbContext db, CancellationToken ct)
        {
            var warningPeriod = DateTime.UtcNow.AddDays(90);

            var contracts = await db.Contracts
                .Where(c => c.EndDate.HasValue && c.EndDate.Value <= warningPeriod && c.EndDate.Value > DateTime.UtcNow && c.Status == "Active")
                .ToListAsync(ct);

            foreach (var c in contracts)
            {
                if (!c.AssignedLawyerId.HasValue) continue;
                var userId = c.AssignedLawyerId.Value;

                var exists = await ExistsRecentAsync(db, "ContractRenewal", "Contract", c.ContractId, userId, ct);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "Contract renewal due",
                    Message = $"Contract {c.ContractNumber} is nearing expiry ({c.EndDate:yyyy-MM-dd}).",
                    Type = "ContractRenewal",
                    EntityType = "Contract",
                    EntityId = c.ContractId,
                    ActionUrl = $"/document-management/contracts/{c.ContractId}",
                    Priority = "High",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task GenerateOverdueResolutionNotifications(LegalCaseDbContext db, CancellationToken ct)
        {
            var now = DateTime.UtcNow.Date;

            var overdue = await db.Resolutions
                .Where(r => (r.Status == "Pending" || r.Status == "InProgress") && r.DueDate != null && r.DueDate < now)
                .ToListAsync(ct);

            foreach (var r in overdue)
            {
                var userId = r.ResponsibleUserId;

                var exists = await ExistsRecentAsync(db, "MeetingAction", "Resolution", r.ResolutionId, userId, ct);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "Overdue meeting action",
                    Message = $"Resolution '{r.Title}' is overdue (due {r.DueDate:yyyy-MM-dd}).",
                    Type = "MeetingAction",
                    EntityType = "Resolution",
                    EntityId = r.ResolutionId,
                    ActionUrl = $"/meetings/{r.MeetingId}",
                    Priority = "High",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task GenerateRetentionExpiryNotifications(LegalCaseDbContext db, CancellationToken ct)
        {
            var warning = DateTime.UtcNow.AddDays(30);

            var docs = await db.Documents
                .Where(d => !d.IsDisposed && !d.IsUnderLegalHold && d.RetentionExpiryDate != null && d.RetentionExpiryDate <= warning)
                .OrderBy(d => d.RetentionExpiryDate)
                .ToListAsync(ct);

            foreach (var d in docs)
            {
                if (string.IsNullOrWhiteSpace(d.UploadedBy)) continue;

                if (!int.TryParse(d.UploadedBy, out var userId))
                {
                    continue;
                }

                var exists = await ExistsRecentAsync(db, "RetentionExpiry", "Document", null, userId, ct);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "Document retention expiring",
                    Message = $"Document '{d.FileName}' retention expires on {d.RetentionExpiryDate:yyyy-MM-dd}.",
                    Type = "RetentionExpiry",
                    EntityType = "Document",
                    EntityId = null,
                    ActionUrl = $"/document-management/view/{d.Id}",
                    Priority = "Medium",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task GenerateUpcomingHearingNotifications(LegalCaseDbContext db, CancellationToken ct)
        {
            var windowStart = DateTime.UtcNow;
            var windowEnd = DateTime.UtcNow.AddDays(7);

            var hearings = await db.Hearings
                .Include(h => h.Case)
                .Where(h => h.Status == "Scheduled" && h.Date >= windowStart.Date && h.Date <= windowEnd.Date)
                .ToListAsync(ct);

            foreach (var h in hearings)
            {
                var userId = h.Case.AssignedLawyerId;

                var exists = await ExistsRecentAsync(db, "CaseHearing", "Case", h.CaseId, userId, ct);
                if (exists) continue;

                db.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "Upcoming case hearing",
                    Message = $"Hearing for case {h.Case.CaseNumber} scheduled on {h.Date:yyyy-MM-dd} at {h.Time}.",
                    Type = "CaseHearing",
                    EntityType = "Case",
                    EntityId = h.CaseId,
                    ActionUrl = $"/case-management/details/{h.CaseId}",
                    Priority = "High",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
