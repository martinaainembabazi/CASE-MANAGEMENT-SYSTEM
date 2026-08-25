using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLawFirmToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LawFirmId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LawFirmId",
                table: "Cases",
                column: "LawFirmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_LawFirms_LawFirmId",
                table: "Cases",
                column: "LawFirmId",
                principalTable: "LawFirms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_LawFirms_LawFirmId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LawFirmId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LawFirmId",
                table: "Cases");
        }
    }
}
