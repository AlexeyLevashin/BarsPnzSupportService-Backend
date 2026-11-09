using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261020000000_AddedRequestMerge")]
public partial class AddedRequestMerge : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "MergedIntoRequestId", table: "Requests", type: "uuid", nullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Requests_Requests_MergedIntoRequestId",
            table: "Requests",
            column: "MergedIntoRequestId",
            principalTable: "Requests",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.CreateIndex(
            name: "IX_Requests_MergedIntoRequestId",
            table: "Requests",
            column: "MergedIntoRequestId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Requests_MergedIntoRequestId", table: "Requests");
        migrationBuilder.DropForeignKey(
            name: "FK_Requests_Requests_MergedIntoRequestId", table: "Requests");
        migrationBuilder.DropColumn(name: "MergedIntoRequestId", table: "Requests");
    }
}
