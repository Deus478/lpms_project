using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class AddSideSpecificLawyersToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefendantLawyerId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlaintiffLawyerId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_DefendantLawyerId",
                table: "Cases",
                column: "DefendantLawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_PlaintiffLawyerId",
                table: "Cases",
                column: "PlaintiffLawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Lawyers_DefendantLawyerId",
                table: "Cases",
                column: "DefendantLawyerId",
                principalTable: "Lawyers",
                principalColumn: "LawyerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Lawyers_PlaintiffLawyerId",
                table: "Cases",
                column: "PlaintiffLawyerId",
                principalTable: "Lawyers",
                principalColumn: "LawyerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Lawyers_DefendantLawyerId",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Lawyers_PlaintiffLawyerId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_DefendantLawyerId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_PlaintiffLawyerId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DefendantLawyerId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "PlaintiffLawyerId",
                table: "Cases");
        }
    }
}
