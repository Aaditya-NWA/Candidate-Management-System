using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CandidateService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MailId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SkillSet = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    AvailabilityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrimarySkillLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_MailId_SkillSet_AvailabilityDate",
                table: "Candidates",
                columns: new[] { "MailId", "SkillSet", "AvailabilityDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidates");
        }
    }
}
