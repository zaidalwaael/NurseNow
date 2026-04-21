using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminApprovalStatusToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminApprovalStatus",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminApprovalStatus",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1354));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1356));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1358));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1360));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1362));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1364));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1366));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 8,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1368));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 9,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1370));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "UpdatedAt",
                value: new DateTime(2026, 4, 15, 21, 38, 20, 206, DateTimeKind.Utc).AddTicks(1371));
        }
    }
}
