using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class AddSidesCourtLevelCaseDatesAndSanitization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Courts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Side",
                table: "CaseParties",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Courts",
                keyColumn: "CourtId",
                keyValue: 1,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "Courts",
                keyColumn: "CourtId",
                keyValue: 2,
                column: "Level",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "CaseParties");
        }
    }
}
