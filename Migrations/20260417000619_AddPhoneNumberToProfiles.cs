using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2506));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2508));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2509));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2512));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2513));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2514));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2515));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2516));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 17, 0, 6, 18, 629, DateTimeKind.Utc).AddTicks(2518));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "PatientProfiles");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4029));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4032));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4034));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4035));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4036));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4037));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4039));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4040));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4041));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 16, 50, 795, DateTimeKind.Utc).AddTicks(4042));
        }
    }
}
