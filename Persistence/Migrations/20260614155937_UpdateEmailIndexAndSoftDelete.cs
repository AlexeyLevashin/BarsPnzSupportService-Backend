using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailIndexAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_OperatorId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_OperatorId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "Requests");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RequestOperators",
                columns: table => new
                {
                    OperatorsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestOperators", x => new { x.OperatorsId, x.RequestsId });
                    table.ForeignKey(
                        name: "FK_RequestOperators_Requests_RequestsId",
                        column: x => x.RequestsId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestOperators_Users_OperatorsId",
                        column: x => x.OperatorsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestOperators_RequestsId",
                table: "RequestOperators",
                column: "RequestsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestOperators");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorId",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_OperatorId",
                table: "Requests",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_OperatorId",
                table: "Requests",
                column: "OperatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
