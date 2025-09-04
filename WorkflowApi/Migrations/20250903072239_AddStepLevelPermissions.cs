using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WorkflowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStepLevelPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StepOrder",
                table: "WorkflowSteps",
                newName: "StepType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkflowSteps",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "WorkflowSteps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "WorkflowSteps",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowId1",
                table: "WorkflowSteps",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Workflows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowStepPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowStepId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    CanExecute = table.Column<bool>(type: "boolean", nullable: false),
                    CanModify = table.Column<bool>(type: "boolean", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStepPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStepPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowStepPermissions_WorkflowSteps_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalTable: "WorkflowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId1",
                table: "WorkflowSteps",
                column: "WorkflowId1");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_CreatedById",
                table: "Workflows",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepPermissions_PermissionId",
                table: "WorkflowStepPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStepPermissions_WorkflowStepId",
                table: "WorkflowStepPermissions",
                column: "WorkflowStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Users_CreatedById",
                table: "Workflows",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Workflows_WorkflowId1",
                table: "WorkflowSteps",
                column: "WorkflowId1",
                principalTable: "Workflows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Users_CreatedById",
                table: "Workflows");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Workflows_WorkflowId1",
                table: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "WorkflowStepPermissions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_WorkflowId1",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_CreatedById",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "WorkflowId1",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Workflows");

            migrationBuilder.RenameColumn(
                name: "StepType",
                table: "WorkflowSteps",
                newName: "StepOrder");
        }
    }
}
