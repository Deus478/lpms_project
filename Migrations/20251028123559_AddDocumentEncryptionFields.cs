using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentEncryptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptionIv",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKey",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptionIv",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EncryptionKey",
                table: "Documents");
        }
    }
}
