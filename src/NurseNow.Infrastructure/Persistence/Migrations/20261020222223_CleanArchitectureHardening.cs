using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitectureHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM WeeklyAvailabilities WHERE LEN(DayOfWeek) > 9)
    THROW 51000, 'Review invalid DayOfWeek values before migration.', 1;
IF EXISTS (SELECT BookingId FROM Reviews GROUP BY BookingId HAVING COUNT(*) > 1)
    THROW 51001, 'Review duplicate booking reviews before migration.', 1;");

            migrationBuilder.DropIndex(
                name: "IX_WeeklyAvailabilities_NurseId",
                table: "WeeklyAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_NurseId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityOverrides_NurseId",
                table: "AvailabilityOverrides");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "WeeklyAvailabilities",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailabilities_NurseId_DayOfWeek_IsActive",
                table: "WeeklyAvailabilities",
                columns: new[] { "NurseId", "DayOfWeek", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_NurseId_BookingDate_StartTime",
                table: "Bookings",
                columns: new[] { "NurseId", "BookingDate", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityOverrides_NurseId_Date",
                table: "AvailabilityOverrides",
                columns: new[] { "NurseId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyAvailabilities_NurseId_DayOfWeek_IsActive",
                table: "WeeklyAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_NurseId_BookingDate_StartTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityOverrides_NurseId_Date",
                table: "AvailabilityOverrides");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "WeeklyAvailabilities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailabilities_NurseId",
                table: "WeeklyAvailabilities",
                column: "NurseId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_NurseId",
                table: "Bookings",
                column: "NurseId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityOverrides_NurseId",
                table: "AvailabilityOverrides",
                column: "NurseId");
        }
    }
}
