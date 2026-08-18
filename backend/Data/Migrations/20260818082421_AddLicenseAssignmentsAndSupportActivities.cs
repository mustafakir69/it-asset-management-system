using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseAssignmentsAndSupportActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Licenses_UsedSeats_NonNegative",
                table: "Licenses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Licenses_UsedSeats_NotGreaterThanTotal",
                table: "Licenses");

            migrationBuilder.RenameColumn(
                name: "UsedSeats",
                table: "Licenses",
                newName: "LegacyUsedSeats");

            migrationBuilder.CreateTable(
                name: "LicenseAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LicenseId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseAssignments", x => x.Id);
                    table.CheckConstraint("CK_LicenseAssignments_RevocationComplete", "([RevokedAt] IS NULL AND [RevokedByUserId] IS NULL) OR ([RevokedAt] IS NOT NULL AND [RevokedByUserId] IS NOT NULL)");
                    table.CheckConstraint("CK_LicenseAssignments_TargetRequired", "[EmployeeId] IS NOT NULL OR [AssetId] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_LicenseAssignments_AppUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseAssignments_AppUsers_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseAssignments_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseAssignments_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportRequestActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MaintenanceRequestId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequestActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportRequestActivities_AppUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportRequestActivities_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Licenses_LegacyUsedSeats_NonNegative",
                table: "Licenses",
                sql: "[LegacyUsedSeats] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAssignments_AssetId",
                table: "LicenseAssignments",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAssignments_AssignedByUserId",
                table: "LicenseAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAssignments_EmployeeId",
                table: "LicenseAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAssignments_RevokedByUserId",
                table: "LicenseAssignments",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_LicenseAssignments_ActiveTarget",
                table: "LicenseAssignments",
                columns: new[] { "LicenseId", "EmployeeId", "AssetId" },
                unique: true,
                filter: "[RevokedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequestActivities_PerformedByUserId",
                table: "SupportRequestActivities",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequestActivities_RequestId_OccurredAt",
                table: "SupportRequestActivities",
                columns: new[] { "MaintenanceRequestId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseAssignments");

            migrationBuilder.DropTable(
                name: "SupportRequestActivities");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Licenses_LegacyUsedSeats_NonNegative",
                table: "Licenses");

            migrationBuilder.RenameColumn(
                name: "LegacyUsedSeats",
                table: "Licenses",
                newName: "UsedSeats");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Licenses_UsedSeats_NonNegative",
                table: "Licenses",
                sql: "[UsedSeats] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Licenses_UsedSeats_NotGreaterThanTotal",
                table: "Licenses",
                sql: "[UsedSeats] <= [TotalSeats]");
        }
    }
}
