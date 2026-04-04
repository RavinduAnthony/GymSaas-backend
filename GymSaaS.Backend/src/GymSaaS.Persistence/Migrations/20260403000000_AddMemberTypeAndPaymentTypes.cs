using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberTypeAndPaymentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Members: add MemberType column ───────────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "MemberType",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "Monthly");

            // ── PaymentTypes: seed Late Monthly (4) and Full Package Payment (5) ─
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 4, "Recurring monthly payment made after Week 1 (days 8+)", true, "Late Monthly" },
                    { 5, "Single full upfront payment for the entire package duration (Type B)", true, "Full Package Payment" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValues: new object[] { 4, 5 });

            migrationBuilder.DropColumn(
                name: "MemberType",
                table: "Members");
        }
    }
}
