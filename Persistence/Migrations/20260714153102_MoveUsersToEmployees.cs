using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveUsersToEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.CreateTable(
        name: "Employees",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "text", nullable: false),
            Surname = table.Column<string>(type: "text", nullable: false),
            Patronymic = table.Column<string>(type: "text", nullable: true),
            PhoneNumber = table.Column<string>(type: "text", nullable: true),
            Email = table.Column<string>(type: "text", nullable: true),
            IsUser = table.Column<bool>(type: "boolean", nullable: false),
            IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Employees", x => x.Id);
        });

    // 2. ПЕРЕНЕСЕНО СНИЗУ: Сначала добавляем колонку в Users
    migrationBuilder.AddColumn<Guid>(
        name: "EmployeeId",
        table: "Users",
        type: "uuid",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

    // 3. Запускаем перенос данных (теперь колонка EmployeeId существует)
    migrationBuilder.Sql(@"
        -- 1. Генерируем уникальные ID и привязываем их к текущим юзерам
        UPDATE ""Users"" 
        SET ""EmployeeId"" = gen_random_uuid();

        -- 2. Создаем карточки сотрудников с этими ID, переносим ФИО и ставим флаг IsUser
        INSERT INTO ""Employees"" (""Id"", ""Name"", ""Surname"", ""Patronymic"", ""IsUser"", ""Email"")
        SELECT ""EmployeeId"", ""Name"", ""Surname"", ""Patronymic"", true, ""Email""
        FROM ""Users"";
    ");

    // 4. ПЕРЕНЕСЕНО СВЕРХУ: Теперь, когда данные совпадают, безопасно вешаем внешний ключ
    migrationBuilder.AddForeignKey(
        name: "FK_Users_Employees_EmployeeId",
        table: "Users",
        column: "EmployeeId",
        principalTable: "Employees",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Patronymic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HeadName",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "HeadPatronymic",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "HeadSurname",
                table: "Institutions");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Institutions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeadId",
                table: "Institutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Institutions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
            
            migrationBuilder.CreateTable(
                name: "JobTitles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeInstitutions",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeInstitutions", x => new { x.EmployeeId, x.InstitutionId });
                    table.ForeignKey(
                        name: "FK_EmployeeInstitutions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeInstitutions_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeInstitutions_JobTitles_JobTitleId",
                        column: x => x.JobTitleId,
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_InstitutionId",
                table: "Requests",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_HeadId",
                table: "Institutions",
                column: "HeadId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInstitutions_InstitutionId",
                table: "EmployeeInstitutions",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInstitutions_JobTitleId",
                table: "EmployeeInstitutions",
                column: "JobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_Employees_HeadId",
                table: "Institutions",
                column: "HeadId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Institutions_InstitutionId",
                table: "Requests",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_Employees_HeadId",
                table: "Institutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Institutions_InstitutionId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "EmployeeInstitutions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmployeeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Requests_InstitutionId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Institutions_HeadId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "HeadId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Institutions");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Patronymic",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeadName",
                table: "Institutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadPatronymic",
                table: "Institutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadSurname",
                table: "Institutions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstitutionId",
                table: "Users",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institutions_InstitutionId",
                table: "Users",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
