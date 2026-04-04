using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainerTypeId",
                table: "Trainers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainerTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_TrainerTypeId",
                table: "Trainers",
                column: "TrainerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_TrainerTypes_TrainerTypeId",
                table: "Trainers",
                column: "TrainerTypeId",
                principalTable: "TrainerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_TrainerTypes_TrainerTypeId",
                table: "Trainers");

            migrationBuilder.DropTable(
                name: "TrainerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_TrainerTypeId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "TrainerTypeId",
                table: "Trainers");
        }
    }
}
