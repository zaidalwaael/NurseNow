using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class CreateSystemSettingsTableV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoAssignNurse = table.Column<bool>(type: "bit", nullable: false),
                    RequireDocumentVerification = table.Column<bool>(type: "bit", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewNurse = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewServiceRequest = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewComplaint = table.Column<bool>(type: "bit", nullable: false),
                    SessionTimeout = table.Column<int>(type: "int", nullable: false),
                    MinimumPasswordLength = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[]
                {
            "AutoAssignNurse",
            "RequireDocumentVerification",
            "EmailNotifications",
            "NotifyNewNurse",
            "NotifyNewServiceRequest",
            "NotifyNewComplaint",
            "SessionTimeout",
            "MinimumPasswordLength"
                },
                values: new object[]
                {
            true,
            true,
            true,
            true,
            true,
            true,
            30,
            8
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
