using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseNow.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCatalogAndRefactorServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "DurationInMinutes",
                table: "Services",
                newName: "ServiceCatalogId");

            migrationBuilder.CreateTable(
                name: "ServiceCatalogs",
                columns: table => new
                {
                    ServiceCatalogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultDurationInMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCatalogs", x => x.ServiceCatalogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceCatalogId",
                table: "Services",
                column: "ServiceCatalogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ServiceCatalogs_ServiceCatalogId",
                table: "Services",
                column: "ServiceCatalogId",
                principalTable: "ServiceCatalogs",
                principalColumn: "ServiceCatalogId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ServiceCatalogs_ServiceCatalogId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "ServiceCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_Services_ServiceCatalogId",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "ServiceCatalogId",
                table: "Services",
                newName: "DurationInMinutes");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
