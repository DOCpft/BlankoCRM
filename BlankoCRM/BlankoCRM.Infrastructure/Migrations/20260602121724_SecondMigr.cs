using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlankoCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentServices_Services_ServiceId",
                table: "AppointmentServices");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentServices_Tenants_TenantId",
                table: "AppointmentServices");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "AppointmentServices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "AppointmentServices",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "AppointmentServices",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentServices_Services_ServiceId",
                table: "AppointmentServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentServices_Tenants_TenantId",
                table: "AppointmentServices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentServices_Services_ServiceId",
                table: "AppointmentServices");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentServices_Tenants_TenantId",
                table: "AppointmentServices");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "AppointmentServices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "AppointmentServices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "AppointmentServices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentServices_Services_ServiceId",
                table: "AppointmentServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentServices_Tenants_TenantId",
                table: "AppointmentServices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
