using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedCanCreateWorkflowPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert the CanCreateWorkflow permission if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT INTO ""Permissions"" (""Name"", ""Description"", ""Resource"", ""Action"")
                SELECT 'CanCreateWorkflow', 'Permission to create new workflows', 'Workflow', 'Create'
                WHERE NOT EXISTS (SELECT 1 FROM ""Permissions"" WHERE ""Name"" = 'CanCreateWorkflow');
            ");

            // Ensure the User role exists
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""Name"", ""Description"")
                SELECT 'User', 'Default user role'
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'User');
            ");

            // Assign CanCreateWorkflow permission to User role
            migrationBuilder.Sql(@"
                INSERT INTO ""RolePermissions"" (""RoleId"", ""PermissionId"")
                SELECT r.""Id"", p.""Id""
                FROM ""Roles"" r
                CROSS JOIN ""Permissions"" p
                WHERE r.""Name"" = 'User' AND p.""Name"" = 'CanCreateWorkflow'
                AND NOT EXISTS (
                    SELECT 1 FROM ""RolePermissions"" rp
                    WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the RolePermission assignment
            migrationBuilder.Sql(@"
                DELETE FROM ""RolePermissions""
                WHERE ""RoleId"" IN (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'User')
                AND ""PermissionId"" IN (SELECT ""Id"" FROM ""Permissions"" WHERE ""Name"" = 'CanCreateWorkflow');
            ");

            // Remove the CanCreateWorkflow permission
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions"" WHERE ""Name"" = 'CanCreateWorkflow';
            ");
        }
    }
}
