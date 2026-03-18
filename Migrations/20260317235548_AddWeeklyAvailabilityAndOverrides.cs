using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyAvailabilityAndOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailabilityOverrides",
                columns: table => new
                {
                    AvailabilityOverrideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NurseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityOverrides", x => x.AvailabilityOverrideId);
                    table.ForeignKey(
                        name: "FK_AvailabilityOverrides_AspNetUsers_NurseId",
                        column: x => x.NurseId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAvailabilities",
                columns: table => new
                {
                    WeeklyAvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NurseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAvailabilities", x => x.WeeklyAvailabilityId);
                    table.ForeignKey(
                        name: "FK_WeeklyAvailabilities_AspNetUsers_NurseId",
                        column: x => x.NurseId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityOverrides_NurseId",
                table: "AvailabilityOverrides",
                column: "NurseId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailabilities_NurseId",
                table: "WeeklyAvailabilities",
                column: "NurseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityOverrides");

            migrationBuilder.DropTable(
                name: "WeeklyAvailabilities");
        }
    }
}
