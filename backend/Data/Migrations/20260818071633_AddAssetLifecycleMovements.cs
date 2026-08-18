using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetLifecycleMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetMovements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMovements_AppUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetMovements_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                "UPDATE [Assets] SET [Status] = N'Boşta' WHERE [Status] = N'Stokta';");
            migrationBuilder.Sql(
                "UPDATE [Assets] SET [Status] = N'Elden Çıkarıldı' WHERE [Status] = N'Elden çıkarıldı';");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Assets_Status_Valid",
                table: "Assets",
                sql: "[Status] IN (N'Boşta', N'Zimmetli', N'Bakımda', N'Kayıp', N'Hurda', N'Elden Çıkarıldı')");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_AssetId_OccurredAt",
                table: "AssetMovements",
                columns: new[] { "AssetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_PerformedByUserId",
                table: "AssetMovements",
                column: "PerformedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Assets_Status_Valid",
                table: "Assets");

            migrationBuilder.Sql(
                "UPDATE [Assets] SET [Status] = N'Stokta' WHERE [Status] = N'Boşta';");
            migrationBuilder.Sql(
                "UPDATE [Assets] SET [Status] = N'Elden çıkarıldı' WHERE [Status] = N'Elden Çıkarıldı';");
        }
    }
}
