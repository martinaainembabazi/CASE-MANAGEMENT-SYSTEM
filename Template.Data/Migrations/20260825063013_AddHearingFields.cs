using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHearingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Hearings",
                newName: "HearingDate");

            migrationBuilder.AddColumn<string>(
                name: "CourtLocation",
                table: "Hearings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JudgeOrMagistrate",
                table: "Hearings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Hearings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Hearings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourtLocation",
                table: "Hearings");

            migrationBuilder.DropColumn(
                name: "JudgeOrMagistrate",
                table: "Hearings");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Hearings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Hearings");

            migrationBuilder.RenameColumn(
                name: "HearingDate",
                table: "Hearings",
                newName: "Date");
        }
    }
}
