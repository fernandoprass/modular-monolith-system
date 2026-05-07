using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                schema: "iam",
                table: "permissions",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "group",
                schema: "iam",
                table: "permissions",
                newName: "resource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "resource",
                schema: "iam",
                table: "permissions",
                newName: "group");

            migrationBuilder.RenameColumn(
                name: "action",
                schema: "iam",
                table: "permissions",
                newName: "name");
        }
    }
}
