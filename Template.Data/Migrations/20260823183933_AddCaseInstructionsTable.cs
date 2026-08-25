using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseInstructionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseInstructions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseAssignmentId = table.Column<int>(type: "int", nullable: false),
                    InstructionsText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseInstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseInstructions_AspNetUsers_SentById",
                        column: x => x.SentById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseInstructions_CaseAssignments_CaseAssignmentId",
                        column: x => x.CaseAssignmentId,
                        principalTable: "CaseAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseInstructions_CaseAssignmentId",
                table: "CaseInstructions",
                column: "CaseAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseInstructions_SentById",
                table: "CaseInstructions",
                column: "SentById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseInstructions");
        }
    }
}
