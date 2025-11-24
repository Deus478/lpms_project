using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cases",
                columns: new[] { "CaseId", "AssignedLawyerId", "CaseNumber", "CourtId", "CreatedAt", "DateFiled", "Description", "EndDate", "IsActive", "Outcome", "StartDate", "Status", "Title", "UpdatedAt" },
                values: new object[] { 1001, 1, "CASE-2025-001", 1, new DateTime(2025, 10, 21, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sample seeded case for demonstration", new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null, new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending", "Alice vs. Bob", null });

            migrationBuilder.InsertData(
                table: "CaseLawyers",
                columns: new[] { "CaseLawyerId", "CaseId", "CreatedAt", "LawyerId" },
                values: new object[] { 3001, 1001, new DateTime(2025, 10, 21, 9, 5, 0, 0, DateTimeKind.Unspecified), 2 });

            migrationBuilder.InsertData(
                table: "CaseParties",
                columns: new[] { "CasePartyId", "CaseId", "CreatedAt", "PartyId", "Role", "Side" },
                values: new object[,]
                {
                    { 2001, 1001, new DateTime(2025, 10, 21, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, "Plaintiff", "Accuser" },
                    { 2002, 1001, new DateTime(2025, 10, 21, 9, 0, 0, 0, DateTimeKind.Unspecified), 2, "Defendant", "Accused" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CaseLawyers",
                keyColumn: "CaseLawyerId",
                keyValue: 3001);

            migrationBuilder.DeleteData(
                table: "CaseParties",
                keyColumn: "CasePartyId",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "CaseParties",
                keyColumn: "CasePartyId",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "Cases",
                keyColumn: "CaseId",
                keyValue: 1001);
        }
    }
}
