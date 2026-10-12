using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20261010000000_AddedRequestType")]
public partial class AddedRequestType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Type",
            table: "Requests",
            type: "text",
            nullable: false,
            defaultValue: "System");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Type", table: "Requests");
    }
}
