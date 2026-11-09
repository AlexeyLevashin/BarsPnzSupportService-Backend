using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261103010000_AddedTwoFactorLockout")]
public partial class AddedTwoFactorLockout : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LockedUntil", table: "TwoFactorAuths", type: "timestamp with time zone", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "LockedUntil", table: "TwoFactorAuths");
    }
}
