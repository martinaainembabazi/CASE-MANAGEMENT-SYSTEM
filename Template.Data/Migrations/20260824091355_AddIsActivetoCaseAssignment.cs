using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActivetoCaseAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CaseAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CaseAssignments");
        }
    }
}
