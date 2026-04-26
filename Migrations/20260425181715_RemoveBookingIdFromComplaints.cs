using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    public partial class RemoveBookingIdFromComplaints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Remove FK
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints");

            // 2. Remove Index
            migrationBuilder.DropIndex(
                name: "IX_Complaints_BookingId",
                table: "Complaints");

            // 3. Remove Column
            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Complaints");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Add Column
            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Complaints",
                type: "int",
                nullable: true);

            // 2. Add Index
            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BookingId",
                table: "Complaints",
                column: "BookingId");

            // 3. Add FK
            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId");
        }
    }
}