using LegalCaseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LegalCaseManagement.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(LegalCaseDbContext context, CancellationToken cancellationToken = default)
        {
            // Courts
            if (!await context.Courts.AnyAsync(cancellationToken))
            {
                context.Courts.AddRange(new[]
                {
                    new Court { Name = "High Court", Type = "Superior", Level = "High", City = "Capital", State = "CA", IsActive = true },
                    new Court { Name = "District Court", Type = "District", Level = "District", City = "Downtown", State = "CA", IsActive = true }
                });
                await context.SaveChangesAsync(cancellationToken);
            }

            // Lawyers
            if (!await context.Lawyers.AnyAsync(cancellationToken))
            {
                context.Lawyers.AddRange(new[]
                {
                    new Lawyer { FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", BarNumber = "BAR1001", Specialization = "Civil", IsActive = true },
                    new Lawyer { FirstName = "John", LastName = "Smith", Email = "john.smith@example.com", BarNumber = "BAR1002", Specialization = "Criminal", IsActive = true }
                });
                await context.SaveChangesAsync(cancellationToken);
            }

            // Parties
            if (!await context.Parties.AnyAsync(cancellationToken))
            {
                context.Parties.AddRange(new[]
                {
                    new Party { FirstName = "Acme", LastName = "Corporation", PartyType = "Organization" },
                    new Party { FirstName = "Globex", LastName = "Inc", PartyType = "Organization" },
                    new Party { FirstName = "Alice", LastName = "Williams", PartyType = "Individual" }
                });
                await context.SaveChangesAsync(cancellationToken);
            }

            // Judges
            if (!await context.Judges.AnyAsync(cancellationToken))
            {
                var courtIds = await context.Courts.Select(c => c.CourtId).ToListAsync(cancellationToken);
                var firstCourtId = courtIds.First();
                var secondCourtId = courtIds.Skip(1).FirstOrDefault(firstCourtId);

                context.Judges.AddRange(new[]
                {
                    new Judge { FirstName = "Emily", LastName = "Carter", Email = "ecarter@court.gov", Phone = "555-0401", Title = "Hon.", CourtId = firstCourtId, Chambers = "A1", Courtroom = "101", AppointmentDate = DateTime.UtcNow.AddYears(-5), IsActive = true },
                    new Judge { FirstName = "Michael", LastName = "Rodriguez", Email = "mrodriguez@court.gov", Phone = "555-0402", Title = "Hon.", CourtId = secondCourtId, Chambers = "B2", Courtroom = "202", AppointmentDate = DateTime.UtcNow.AddYears(-3), IsActive = true }
                });
                await context.SaveChangesAsync(cancellationToken);
            }

            // Cases (with parties, hearings, deadlines)
            if (!await context.Cases.AnyAsync(cancellationToken))
            {
                var lawyerId = await context.Lawyers.Select(l => l.LawyerId).FirstAsync(cancellationToken);
                var courtId = await context.Courts.Select(c => c.CourtId).FirstAsync(cancellationToken);

                var case1 = new Case
                {
                    CaseNumber = "CASE-2025-0001",
                    Title = "Acme vs Globex",
                    Description = "Breach of contract",
                    AssignedLawyerId = lawyerId,
                    CourtId = courtId,
                    DateFiled = DateTime.UtcNow.Date.AddDays(-14),
                    StartDate = DateTime.UtcNow.Date.AddDays(-7),
                    Status = "Active"
                };
                context.Cases.Add(case1);
                await context.SaveChangesAsync(cancellationToken);

                // Link parties with sides
                var acmeId = await context.Parties.Where(p => p.FirstName == "Acme").Select(p => p.PartyId).FirstAsync(cancellationToken);
                var globexId = await context.Parties.Where(p => p.FirstName == "Globex").Select(p => p.PartyId).FirstAsync(cancellationToken);

                context.CaseParties.AddRange(new[]
                {
                    new CaseParty { CaseId = case1.CaseId, PartyId = acmeId, Role = "Plaintiff", Side = "Accuser" },
                    new CaseParty { CaseId = case1.CaseId, PartyId = globexId, Role = "Defendant", Side = "Accused" }
                });

                // Hearing and deadline
                context.Hearings.Add(new Hearing
                {
                    CaseId = case1.CaseId,
                    CourtId = courtId,
                    Date = DateTime.UtcNow.Date.AddDays(7),
                    Time = new TimeSpan(10, 0, 0),
                    Location = "Courtroom 1",
                    HearingType = "Initial",
                    Status = "Scheduled"
                });

                context.Deadlines.Add(new Deadline
                {
                    CaseId = case1.CaseId,
                    DueDate = DateTime.UtcNow.Date.AddDays(10),
                    Description = "Submit evidence bundle",
                    Priority = "High",
                    IsCompleted = false
                });

                await context.SaveChangesAsync(cancellationToken);
            }

            // Governance: Boards, Committees, Meetings
            if (!await context.Boards.AnyAsync(cancellationToken))
            {
                var board = new Board { Name = "Main Board", Description = "Corporate Board", IsActive = true };
                context.Boards.Add(board);
                await context.SaveChangesAsync(cancellationToken);

                var committee = new Committee { BoardId = board.BoardId, Name = "Audit Committee", Description = "Oversight" };
                context.Committees.Add(committee);
                await context.SaveChangesAsync(cancellationToken);

                var meeting = new Meeting
                {
                    BoardId = board.BoardId,
                    ScheduledDate = DateTime.UtcNow.Date.AddDays(5),
                    Location = "HQ Boardroom",
                    Title = "Q4 Planning",
                    Agenda = "Budget; Risk; Strategy",
                    Status = "Scheduled"
                };
                context.Meetings.Add(meeting);
                await context.SaveChangesAsync(cancellationToken);

                context.MeetingAttendances.Add(new MeetingAttendance
                {
                    MeetingId = meeting.MeetingId,
                    AttendeeName = "Jane Doe",
                    AttendeeRole = "Director",
                    Present = true
                });

                context.Minutes.Add(new Minute
                {
                    MeetingId = meeting.MeetingId,
                    Content = "Discussed quarterly targets and risk posture.",
                    RecordedDate = DateTime.UtcNow
                });

                context.Resolutions.Add(new Resolution
                {
                    MeetingId = meeting.MeetingId,
                    Title = "Approve Budget",
                    Description = "Approve FY budget",
                    Status = "Pending",
                    DueDate = DateTime.UtcNow.Date.AddDays(30),
                    ResponsibleUserId = 1 // Default to first user
                });

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}


