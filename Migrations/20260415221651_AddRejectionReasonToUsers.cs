using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2393));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2395));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2396));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2398));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2399));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2400));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2401));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2402));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2403));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 22, 6, 35, 703, DateTimeKind.Utc).AddTicks(2405));
        }
    }
}
