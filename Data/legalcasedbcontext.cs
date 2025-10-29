 using Microsoft.EntityFrameworkCore;
using LegalCaseManagement.Models;

namespace LegalCaseManagement.Data
{
    /// <summary>
    /// Entity Framework DbContext for Legal Case Management System
    /// </summary>
    public class LegalCaseDbContext : DbContext
    {
        public LegalCaseDbContext(DbContextOptions<LegalCaseDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Case> Cases { get; set; }
        public DbSet<Lawyer> Lawyers { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<CaseParty> CaseParties { get; set; }
        public DbSet<Hearing> Hearings { get; set; }
        public DbSet<Deadline> Deadlines { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingAttendance> MeetingAttendances { get; set; }
        public DbSet<Minute> Minutes { get; set; }
        public DbSet<Resolution> Resolutions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Case entity
            modelBuilder.Entity<Case>(entity =>
            {
                entity.HasKey(e => e.CaseId);
                entity.Property(e => e.CaseNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Active");
                entity.Property(e => e.Outcome).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for case number (should be unique)
                entity.HasIndex(e => e.CaseNumber).IsUnique();

                // Configure relationships
                entity.HasOne(e => e.AssignedLawyer)
                      .WithMany(l => l.Cases)
                      .HasForeignKey(e => e.AssignedLawyerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Court)
                      .WithMany(c => c.Cases)
                      .HasForeignKey(e => e.CourtId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Lawyer entity
            modelBuilder.Entity<Lawyer>(entity =>
            {
                entity.HasKey(e => e.LawyerId);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.BarNumber).HasMaxLength(50);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for email (should be unique)
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.BarNumber).IsUnique();
            });

            // Configure Court entity
            modelBuilder.Entity<Court>(entity =>
            {
                entity.HasKey(e => e.CourtId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Level).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Party entity
            modelBuilder.Entity<Party>(entity =>
            {
                entity.HasKey(e => e.PartyId);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PartyType).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure CaseParty junction table
            modelBuilder.Entity<CaseParty>(entity =>
            {
                entity.HasKey(e => e.CasePartyId);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Side).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure many-to-many relationship
                entity.HasOne(e => e.Case)
                      .WithMany(c => c.CaseParties)
                      .HasForeignKey(e => e.CaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Party)
                      .WithMany(p => p.CaseParties)
                      .HasForeignKey(e => e.PartyId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Composite index to prevent duplicate case-party combinations
                entity.HasIndex(e => new { e.CaseId, e.PartyId, e.Role }).IsUnique();
            });

            // Configure Hearing entity
            modelBuilder.Entity<Hearing>(entity =>
            {
                entity.HasKey(e => e.HearingId);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.HearingType).HasMaxLength(100);
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(e => e.Case)
                      .WithMany(c => c.Hearings)
                      .HasForeignKey(e => e.CaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Court)
                      .WithMany(c => c.Hearings)
                      .HasForeignKey(e => e.CourtId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Deadline entity
            modelBuilder.Entity<Deadline>(entity =>
            {
                entity.HasKey(e => e.DeadlineId);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Priority).HasMaxLength(50).HasDefaultValue("Medium");
                entity.Property(e => e.IsCompleted).HasDefaultValue(false);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship
                entity.HasOne(e => e.Case)
                      .WithMany(c => c.Deadlines)
                      .HasForeignKey(e => e.CaseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileExtension).IsRequired();
                entity.Property(e => e.StoragePath).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UploadedBy).IsRequired();
                entity.Property(e => e.DocumentType).HasMaxLength(100);
                entity.Property(e => e.UploadedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.FileHash);
            });

            // Configure Board entity
            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(e => e.BoardId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Committee entity
            modelBuilder.Entity<Committee>(entity =>
            {
                entity.HasKey(e => e.CommitteeId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Board)
                      .WithMany(b => b.Committees)
                      .HasForeignKey(e => e.BoardId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Meeting entity
            modelBuilder.Entity<Meeting>(entity =>
            {
                entity.HasKey(e => e.MeetingId);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Agenda).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Board)
                      .WithMany(b => b.Meetings)
                      .HasForeignKey(e => e.BoardId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Committee)
                      .WithMany(c => c.Meetings)
                      .HasForeignKey(e => e.CommitteeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure MeetingAttendance entity
            modelBuilder.Entity<MeetingAttendance>(entity =>
            {
                entity.HasKey(e => e.MeetingAttendanceId);
                entity.Property(e => e.AttendeeName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AttendeeRole).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Meeting)
                      .WithMany(m => m.Attendances)
                      .HasForeignKey(e => e.MeetingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Minute entity
            modelBuilder.Entity<Minute>(entity =>
            {
                entity.HasKey(e => e.MinuteId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.RecordedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Meeting)
                      .WithMany(m => m.Minutes)
                      .HasForeignKey(e => e.MeetingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Resolution entity
            modelBuilder.Entity<Resolution>(entity =>
            {
                entity.HasKey(e => e.ResolutionId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(4000);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.ResponsibleParty).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Meeting)
                      .WithMany(m => m.Resolutions)
                      .HasForeignKey(e => e.MeetingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data for testing
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Lawyers
            modelBuilder.Entity<Lawyer>().HasData(
                new Lawyer
                {
                    LawyerId = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@lawfirm.com",
                    Phone = "555-0101",
                    BarNumber = "BAR001",
                    Specialization = "Criminal Law",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                },
                new Lawyer
                {
                    LawyerId = 2,
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@lawfirm.com",
                    Phone = "555-0102",
                    BarNumber = "BAR002",
                    Specialization = "Civil Law",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                }
            );

            // Seed Courts
            modelBuilder.Entity<Court>().HasData(
                new Court
                {
                    CourtId = 1,
                    Name = "Superior Court of Justice",
                    Type = "Superior",
                    Address = "123 Justice Blvd",
                    City = "Downtown",
                    State = "CA",
                    ZipCode = "90210",
                    Phone = "555-0201",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                },
                new Court
                {
                    CourtId = 2,
                    Name = "District Court",
                    Type = "District",
                    Address = "456 Court Street",
                    City = "Midtown",
                    State = "CA",
                    ZipCode = "90211",
                    Phone = "555-0202",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                }
            );

            // Seed Parties
            modelBuilder.Entity<Party>().HasData(
                new Party
                {
                    PartyId = 1,
                    FirstName = "Alice",
                    LastName = "Williams",
                    PartyType = "Individual",
                    Email = "alice.williams@email.com",
                    Phone = "555-0301",
                    Address = "789 Main Street",
                    City = "Downtown",
                    State = "CA",
                    ZipCode = "90210",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                },
                new Party
                {
                    PartyId = 2,
                    FirstName = "Bob",
                    LastName = "Davis",
                    PartyType = "Individual",
                    Email = "bob.davis@email.com",
                    Phone = "555-0302",
                    Address = "321 Oak Avenue",
                    City = "Uptown",
                    State = "CA",
                    ZipCode = "90212",
                    CreatedAt = new DateTime(2025, 10, 20, 15, 30, 0)
                }
            );
        }
    }
}