using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlankoCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientContacts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientContacts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "ClientContacts" ("Id", "OrganizationId", "ClientId", "Type", "Value", "CreatedAt", "UpdatedAt")
                SELECT md5("Id"::text || ':telegram')::uuid, "OrganizationId", "Id", 0, btrim("Telegram"), "CreatedAt", "UpdatedAt"
                FROM "Clients"
                WHERE "Telegram" IS NOT NULL AND btrim("Telegram") <> '';
                """);

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Phone",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Telegram",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Phone",
                table: "Clients",
                columns: new[] { "OrganizationId", "Phone" },
                unique: true,
                filter: "\"Phone\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_ClientId_Type",
                table: "ClientContacts",
                columns: new[] { "ClientId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_OrganizationId",
                table: "ClientContacts",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Phone",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "Telegram",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Clients" AS clients
                SET "Telegram" = contacts."Value"
                FROM "ClientContacts" AS contacts
                WHERE contacts."ClientId" = clients."Id" AND contacts."Type" = 0;
                """);

            migrationBuilder.DropTable(
                name: "ClientContacts");

            migrationBuilder.Sql("""
                UPDATE "Clients"
                SET "Phone" = ''
                WHERE "Phone" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Phone",
                table: "Clients",
                columns: new[] { "OrganizationId", "Phone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Telegram",
                table: "Clients",
                column: "Telegram");
        }
    }
}
