using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    public partial class Fixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ FIX: Drop FK only if exists
            migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_Complaints_Bookings_BookingId'
            )
            BEGIN
                ALTER TABLE [Complaints] DROP CONSTRAINT [FK_Complaints_Bookings_BookingId];
            END
            ");

            // ✅ FIX: Drop index only if exists
            migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1 FROM sys.indexes 
                WHERE name = 'IX_Complaints_BookingId'
                  AND object_id = OBJECT_ID(N'[Complaints]')
            )
            BEGIN
                DROP INDEX [IX_Complaints_BookingId] ON [Complaints];
            END
            ");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Complaints");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "NurseProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentByAdminId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentByAdminName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetAudience",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminResponse",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Complaints",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "Complaints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Complaints",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "AdminApprovalStatus",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminRole",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NurseDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NurseId = table.Column<string>(nullable: false),
                    DocumentName = table.Column<string>(nullable: false),
                    FileUrl = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: false),
                    Message = table.Column<string>(nullable: false),
                    ScheduledTime = table.Column<DateTime>(nullable: false),
                    IsSent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoAssignNurse = table.Column<bool>(nullable: false),
                    RequireDocumentVerification = table.Column<bool>(nullable: false),
                    EmailNotifications = table.Column<bool>(nullable: false),
                    NotifyNewNurse = table.Column<bool>(nullable: false),
                    NotifyNewServiceRequest = table.Column<bool>(nullable: false),
                    NotifyNewComplaint = table.Column<bool>(nullable: false),
                    SessionTimeout = table.Column<int>(nullable: false),
                    MinimumPasswordLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_UserId",
                table: "Complaints",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_AspNetUsers_UserId",
                table: "Complaints",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_AspNetUsers_UserId",
                table: "Complaints");

            migrationBuilder.DropTable(name: "NurseDocuments");
            migrationBuilder.DropTable(name: "ScheduledNotifications");
            migrationBuilder.DropTable(name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_UserId",
                table: "Complaints");

            migrationBuilder.DropColumn(name: "PaymentMethod", table: "Payments");
            migrationBuilder.DropColumn(name: "PhoneNumber", table: "PatientProfiles");
            migrationBuilder.DropColumn(name: "RejectionReason", table: "NurseProfiles");
            migrationBuilder.DropColumn(name: "SentByAdminId", table: "Notifications");
            migrationBuilder.DropColumn(name: "SentByAdminName", table: "Notifications");
            migrationBuilder.DropColumn(name: "TargetAudience", table: "Notifications");

            migrationBuilder.DropColumn(name: "AdminResponse", table: "Complaints");
            migrationBuilder.DropColumn(name: "Category", table: "Complaints");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Complaints");
            migrationBuilder.DropColumn(name: "RespondedAt", table: "Complaints");
            migrationBuilder.DropColumn(name: "Subject", table: "Complaints");
            migrationBuilder.DropColumn(name: "UserId", table: "Complaints");

            migrationBuilder.DropColumn(name: "CreatedAt", table: "Bookings");

            migrationBuilder.DropColumn(name: "AdminApprovalStatus", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "AdminRole", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LastLoginAt", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "Location", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "RejectionReason", table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BookingId",
                table: "Complaints",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}