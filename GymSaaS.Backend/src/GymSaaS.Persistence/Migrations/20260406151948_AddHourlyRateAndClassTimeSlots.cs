using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRateAndClassTimeSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "GymClasses",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ClassTimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    GymClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EndTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTimeSlots_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTimeSlots_GymClasses_GymClassId",
                        column: x => x.GymClassId,
                        principalTable: "GymClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimeSlots_BranchId",
                table: "ClassTimeSlots",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimeSlots_GymClassId",
                table: "ClassTimeSlots",
                column: "GymClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassTimeSlots");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "GymClasses");
        }
    }
}
