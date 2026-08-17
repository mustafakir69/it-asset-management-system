using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceFinalRoleIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM [AppUsers] WHERE [Role] = N'Auditor';");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppUsers_ActiveEmployee_Link",
                table: "AppUsers",
                sql: "[IsActive] = 0 OR [Role] = 'Admin' OR [EmployeeId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppUsers_Role_Valid",
                table: "AppUsers",
                sql: "[Role] IN ('Admin', 'IT', 'Employee')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AppUsers_ActiveEmployee_Link",
                table: "AppUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppUsers_Role_Valid",
                table: "AppUsers");
        }
    }
}
