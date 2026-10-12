using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261013000000_AddedInboundEmail")]
public partial class AddedInboundEmail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Name", table: "Employees", type: "text", nullable: true,
            oldClrType: typeof(string), oldType: "text");
        migrationBuilder.AlterColumn<string>(
            name: "Surname", table: "Employees", type: "text", nullable: true,
            oldClrType: typeof(string), oldType: "text");
        migrationBuilder.AddColumn<bool>(
            name: "IsActivated", table: "Users", type: "boolean", nullable: false, defaultValue: true);
        migrationBuilder.CreateTable(
            name: "InboundEmails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UidValidity = table.Column<long>(type: "bigint", nullable: false),
                Uid = table.Column<long>(type: "bigint", nullable: false),
                RequestId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InboundEmails", x => x.Id);
                table.ForeignKey("FK_InboundEmails_Requests_RequestId", x => x.RequestId,
                    "Requests", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_InboundEmails_RequestId", table: "InboundEmails",
            column: "RequestId");
        migrationBuilder.CreateIndex(name: "IX_InboundEmails_UidValidity_Uid", table: "InboundEmails",
            columns: new[] { "UidValidity", "Uid" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InboundEmails");
        migrationBuilder.DropColumn(name: "IsActivated", table: "Users");
        migrationBuilder.Sql("UPDATE \"Employees\" SET \"Name\" = '' WHERE \"Name\" IS NULL");
        migrationBuilder.Sql("UPDATE \"Employees\" SET \"Surname\" = '' WHERE \"Surname\" IS NULL");
        migrationBuilder.AlterColumn<string>(name: "Name", table: "Employees", type: "text", nullable: false,
            oldClrType: typeof(string), oldType: "text", oldNullable: true);
        migrationBuilder.AlterColumn<string>(name: "Surname", table: "Employees", type: "text", nullable: false,
            oldClrType: typeof(string), oldType: "text", oldNullable: true);
    }
}
