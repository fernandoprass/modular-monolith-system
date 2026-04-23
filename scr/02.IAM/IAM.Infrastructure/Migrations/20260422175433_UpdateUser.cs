using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_organization_admin",
                schema: "iam",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_out_until",
                schema: "iam",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "num_failed_login_attempts",
                schema: "iam",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_organization_admin",
                schema: "iam",
                table: "users");

            migrationBuilder.DropColumn(
                name: "locked_out_until",
                schema: "iam",
                table: "users");

            migrationBuilder.DropColumn(
                name: "num_failed_login_attempts",
                schema: "iam",
                table: "users");
        }
    }
}
