using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlankoCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PublicAppointmentConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Organizations"
                SET "Slug" = 'org-' || replace("Id"::text, '-', '')
                WHERE "Slug" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Slug",
                table: "Organizations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_OrganizationId_Type_Value",
                table: "ClientContacts",
                columns: new[] { "OrganizationId", "Type", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status_ExpiresAt",
                table: "Appointments",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql("""
                ALTER TABLE "Appointments"
                ADD CONSTRAINT "EX_Appointments_NoOverlappingActive"
                EXCLUDE USING gist (
                    "OrganizationId" WITH =,
                    (COALESCE("SalonId", '00000000-0000-0000-0000-000000000000'::uuid)) WITH =,
                    tstzrange("StartTime", "EndTime", '[)') WITH &&
                )
                WHERE ("Status" IN (0, 1, 3));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Appointments\" DROP CONSTRAINT \"EX_Appointments_NoOverlappingActive\";");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Slug",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_ClientContacts_OrganizationId_Type_Value",
                table: "ClientContacts");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status_ExpiresAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Appointments");
        }
    }
}
