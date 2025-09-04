using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WorkflowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EditPermissionId",
                table: "Workflows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExecutePermissionId",
                table: "Workflows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagePermissionId",
                table: "Workflows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewPermissionId",
                table: "Workflows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    PermissionType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowPermissions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowId = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "text", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiredPermissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Permissions_RequiredPermissionId",
                        column: x => x.RequiredPermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_EditPermissionId",
                table: "Workflows",
                column: "EditPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ExecutePermissionId",
                table: "Workflows",
                column: "ExecutePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ManagePermissionId",
                table: "Workflows",
                column: "ManagePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ViewPermissionId",
                table: "Workflows",
                column: "ViewPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowPermissions_PermissionId",
                table: "WorkflowPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowPermissions_WorkflowId",
                table: "WorkflowPermissions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_RequiredPermissionId",
                table: "WorkflowSteps",
                column: "RequiredPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId",
                table: "WorkflowSteps",
                column: "WorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Permissions_EditPermissionId",
                table: "Workflows",
                column: "EditPermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Permissions_ExecutePermissionId",
                table: "Workflows",
                column: "ExecutePermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Permissions_ManagePermissionId",
                table: "Workflows",
                column: "ManagePermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Permissions_ViewPermissionId",
                table: "Workflows",
                column: "ViewPermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Permissions_EditPermissionId",
                table: "Workflows");

            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Permissions_ExecutePermissionId",
                table: "Workflows");

            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Permissions_ManagePermissionId",
                table: "Workflows");

            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Permissions_ViewPermissionId",
                table: "Workflows");

            migrationBuilder.DropTable(
                name: "WorkflowPermissions");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_EditPermissionId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_ExecutePermissionId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_ManagePermissionId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_ViewPermissionId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "EditPermissionId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "ExecutePermissionId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "ManagePermissionId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "ViewPermissionId",
                table: "Workflows");
        }
    }
}
