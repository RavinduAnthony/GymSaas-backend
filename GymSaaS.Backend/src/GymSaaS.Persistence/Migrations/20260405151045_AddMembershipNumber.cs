using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add as nullable first so existing rows don't violate NOT NULL
            migrationBuilder.AddColumn<string>(
                name: "MembershipNumber",
                table: "Members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Assign sequential membership numbers per tenant for existing rows
            migrationBuilder.Sql(@"
                UPDATE ""Members"" m
                SET ""MembershipNumber"" = LPAD(seq.row_num::text, 4, '0')
                FROM (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (PARTITION BY ""TenantId"" ORDER BY ""CreatedAt"", ""Id"") AS row_num
                    FROM ""Members""
                ) seq
                WHERE m.""Id"" = seq.""Id"";
            ");

            // Now make it NOT NULL and add unique index
            migrationBuilder.AlterColumn<string>(
                name: "MembershipNumber",
                table: "Members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_MembershipNumber",
                table: "Members",
                columns: new[] { "TenantId", "MembershipNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_MembershipNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MembershipNumber",
                table: "Members");
        }
    }
}
