using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261013000001_AddedInboundEmailMessageId")]
public partial class AddedInboundEmailMessageId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MessageId", table: "InboundEmails", type: "character varying(512)", maxLength: 512, nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_InboundEmails_MessageId", table: "InboundEmails", column: "MessageId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_InboundEmails_MessageId", table: "InboundEmails");
        migrationBuilder.DropColumn(name: "MessageId", table: "InboundEmails");
    }
}
