using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "CreatedAt", "Email", "EmployeeId", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { "app-user-admin", new DateTimeOffset(new DateTime(2026, 8, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "admin.demo@example.test", null, true, null, "AQAAAAIAAYagAAAAEMWmnyeBUMAPR3BMaPAoDlzXnRlCCuCrvk4eCLxK4dXDv3HDaUF2agezRs6Hfp5wyg==", "Admin", "admin.demo" },
                    { "app-user-auditor", new DateTimeOffset(new DateTime(2026, 8, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "auditor.demo@example.test", null, true, null, "AQAAAAIAAYagAAAAEMWmnyeBUMAPR3BMaPAoDlzXnRlCCuCrvk4eCLxK4dXDv3HDaUF2agezRs6Hfp5wyg==", "Auditor", "auditor.demo" },
                    { "app-user-employee", new DateTimeOffset(new DateTime(2026, 8, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee.demo@example.test", "employee-db-001", true, null, "AQAAAAIAAYagAAAAEMWmnyeBUMAPR3BMaPAoDlzXnRlCCuCrvk4eCLxK4dXDv3HDaUF2agezRs6Hfp5wyg==", "Employee", "employee.demo" },
                    { "app-user-inactive", new DateTimeOffset(new DateTime(2026, 8, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "inactive.demo@example.test", "employee-db-009", false, null, "AQAAAAIAAYagAAAAEMWmnyeBUMAPR3BMaPAoDlzXnRlCCuCrvk4eCLxK4dXDv3HDaUF2agezRs6Hfp5wyg==", "Employee", "inactive.demo" },
                    { "app-user-it", new DateTimeOffset(new DateTime(2026, 8, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "it.demo@example.test", null, true, null, "AQAAAAIAAYagAAAAEMWmnyeBUMAPR3BMaPAoDlzXnRlCCuCrvk4eCLxK4dXDv3HDaUF2agezRs6Hfp5wyg==", "IT", "it.demo" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_AppUsers_Email",
                table: "AppUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AppUsers_EmployeeId",
                table: "AppUsers",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
