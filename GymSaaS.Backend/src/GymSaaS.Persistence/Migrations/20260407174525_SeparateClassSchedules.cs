using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateClassSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassTimeSlots");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "GymClasses");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "GymClasses");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "GymClasses");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "GymClasses");

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GymClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EndTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_GymClasses_GymClassId",
                        column: x => x.GymClassId,
                        principalTable: "GymClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_BranchId",
                table: "ClassSchedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_GymClassId",
                table: "ClassSchedules",
                column: "GymClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                table: "GymClasses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "GymClasses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "GymClasses",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "GymClasses",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClassTimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    GymClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EndTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    StartTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
    }
}
