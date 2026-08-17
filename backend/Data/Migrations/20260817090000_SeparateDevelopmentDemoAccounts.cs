using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TakipProgrami.Api.Data;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260817090000_SeparateDevelopmentDemoAccounts")]
public partial class SeparateDevelopmentDemoAccounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [AppUsers]
            SET [IsActive] = 0,
                [PasswordHash] = '$DEVELOPMENT_ACCOUNT_DISABLED$'
            WHERE [Id] IN ('app-user-admin', 'app-user-it', 'app-user-employee', 'app-user-inactive');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Eski ortak demo kimlik bilgileri güvenlik nedeniyle geri yüklenmez.
    }
}
