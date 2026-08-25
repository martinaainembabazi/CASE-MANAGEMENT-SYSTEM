using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveFlagToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

           // migrationBuilder.AddColumn<string>(
                //name: "InstructionsText",
               //table: "CaseAssignments",
                //type: "nvarchar(max)",
                //nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "InstructionsText",
                table: "CaseAssignments");
        }
    }
}
