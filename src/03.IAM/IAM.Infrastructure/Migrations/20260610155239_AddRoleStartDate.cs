using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleStartDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "starts_at",
                schema: "iam",
                table: "user_roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Local));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "starts_at",
                schema: "iam",
                table: "user_roles");
        }
    }
}
