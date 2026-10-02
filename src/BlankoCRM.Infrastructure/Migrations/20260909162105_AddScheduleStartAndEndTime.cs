using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlankoCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleStartAndEndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Appointments_AppointmentId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Salons_SalonId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_AppointmentId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "SlotTime",
                table: "Schedules",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("""
                UPDATE "Schedules" AS schedule
                SET "EndTime" = schedule."StartTime" +
                    COALESCE((
                        SELECT GREATEST(SUM(appointment_service."DurationMinutes" * appointment_service."Quantity"), 1)
                        FROM "Appointments" AS appointment
                        INNER JOIN "AppointmentServices" AS appointment_service
                            ON appointment_service."AppointmentId" = appointment."Id"
                        WHERE appointment."Id" = schedule."AppointmentId"
                    ), 1) * INTERVAL '1 minute';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_AppointmentId",
                table: "Schedules",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_OrganizationId_SalonId_StartTime",
                table: "Schedules",
                columns: new[] { "OrganizationId", "SalonId", "StartTime" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schedule_EndTime",
                table: "Schedules",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Appointments_AppointmentId",
                table: "Schedules",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Salons_SalonId",
                table: "Schedules",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Appointments_AppointmentId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Salons_SalonId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_AppointmentId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_OrganizationId_SalonId_StartTime",
                table: "Schedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schedule_EndTime",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Schedules",
                newName: "SlotTime");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_AppointmentId",
                table: "Schedules",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Appointments_AppointmentId",
                table: "Schedules",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Salons_SalonId",
                table: "Schedules",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id");
        }
    }
}
