using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lpms_server.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseWorkflowModelsWithFixedCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflows_WorkflowTemplates_WorkflowTemplateId",
                table: "CaseWorkflows");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflowSteps_Users_AssignedUserId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflowSteps_WorkflowStepTemplates_WorkflowStepTemplateId",
                table: "CaseWorkflowSteps");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseWorkflowSteps",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CaseWorkflowSteps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CaseWorkflowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "CaseWorkflows",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CaseId1",
                table: "CaseWorkflows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseWorkflows",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CaseWorkflows",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CaseWorkflows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowSteps_UserId",
                table: "CaseWorkflowSteps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflows_CaseId1",
                table: "CaseWorkflows",
                column: "CaseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflows_Cases_CaseId1",
                table: "CaseWorkflows",
                column: "CaseId1",
                principalTable: "Cases",
                principalColumn: "CaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflows_WorkflowTemplates_WorkflowTemplateId",
                table: "CaseWorkflows",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "WorkflowTemplateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflowSteps_Users_AssignedUserId",
                table: "CaseWorkflowSteps",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflowSteps_Users_UserId",
                table: "CaseWorkflowSteps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflowSteps_WorkflowStepTemplates_WorkflowStepTemplateId",
                table: "CaseWorkflowSteps",
                column: "WorkflowStepTemplateId",
                principalTable: "WorkflowStepTemplates",
                principalColumn: "WorkflowStepTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflows_Cases_CaseId1",
                table: "CaseWorkflows");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflows_WorkflowTemplates_WorkflowTemplateId",
                table: "CaseWorkflows");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflowSteps_Users_AssignedUserId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflowSteps_Users_UserId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseWorkflowSteps_WorkflowStepTemplates_WorkflowStepTemplateId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_CaseWorkflowSteps_UserId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_CaseWorkflows_CaseId1",
                table: "CaseWorkflows");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CaseWorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CaseId1",
                table: "CaseWorkflows");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CaseWorkflows");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CaseWorkflows");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseWorkflows");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "CaseWorkflows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflows_WorkflowTemplates_WorkflowTemplateId",
                table: "CaseWorkflows",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "WorkflowTemplateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflowSteps_Users_AssignedUserId",
                table: "CaseWorkflowSteps",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseWorkflowSteps_WorkflowStepTemplates_WorkflowStepTemplateId",
                table: "CaseWorkflowSteps",
                column: "WorkflowStepTemplateId",
                principalTable: "WorkflowStepTemplates",
                principalColumn: "WorkflowStepTemplateId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
