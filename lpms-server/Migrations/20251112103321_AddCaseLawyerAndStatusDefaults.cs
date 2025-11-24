using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseLawyerAndStatusDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecurrenceDays",
                table: "Meetings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndDate",
                table: "Meetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceInterval",
                table: "Meetings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceType",
                table: "Meetings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeriesId",
                table: "Meetings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "CaseLawyers",
                columns: table => new
                {
                    CaseLawyerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    LawyerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseLawyers", x => x.CaseLawyerId);
                    table.ForeignKey(
                        name: "FK_CaseLawyers_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "CaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseLawyers_Lawyers_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "Lawyers",
                        principalColumn: "LawyerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_SeriesId",
                table: "Meetings",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLawyers_CaseId_LawyerId",
                table: "CaseLawyers",
                columns: new[] { "CaseId", "LawyerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseLawyers_LawyerId",
                table: "CaseLawyers",
                column: "LawyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseLawyers");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_SeriesId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "RecurrenceDays",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "RecurrenceInterval",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "RecurrenceType",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "Meetings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending");
        }
    }
}
