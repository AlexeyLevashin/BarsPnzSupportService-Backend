using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261103000000_AddedTwoFactorAuth")]
public partial class AddedTwoFactorAuth : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TwoFactorAuths",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Secret = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastUsedStep = table.Column<long>(type: "bigint", nullable: false),
                FailedAttempts = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TwoFactorAuths", x => x.Id);
                table.ForeignKey(
                    name: "FK_TwoFactorAuths_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TwoFactorAuths_UserId",
            table: "TwoFactorAuths",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "TwoFactorRecoveryCodes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TwoFactorAuthId = table.Column<Guid>(type: "uuid", nullable: false),
                CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                IsUsed = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TwoFactorRecoveryCodes", x => x.Id);
                table.ForeignKey(
                    name: "FK_TwoFactorRecoveryCodes_TwoFactorAuths_TwoFactorAuthId",
                    column: x => x.TwoFactorAuthId,
                    principalTable: "TwoFactorAuths",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TwoFactorRecoveryCodes_TwoFactorAuthId",
            table: "TwoFactorRecoveryCodes",
            column: "TwoFactorAuthId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TwoFactorRecoveryCodes");
        migrationBuilder.DropTable(name: "TwoFactorAuths");
    }
}
