using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_master",
                schema: "iam",
                table: "organizations");

            migrationBuilder.AddColumn<string>(
                name: "language",
                schema: "iam",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "default_language",
                schema: "iam",
                table: "organizations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "language",
                schema: "iam",
                table: "users");

            migrationBuilder.DropColumn(
                name: "default_language",
                schema: "iam",
                table: "organizations");

            migrationBuilder.AddColumn<bool>(
                name: "is_master",
                schema: "iam",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
