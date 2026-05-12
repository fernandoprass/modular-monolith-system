using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "parameters",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    key = table.Column<string>(type: "character varying(402)", maxLength: 402, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    validation_regex = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    validation_error_custom_message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    list_items = table.Column<string>(type: "text", nullable: true),
                    external_list_endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    override_type = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parameter_overrides",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parameter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter_overrides", x => x.id);
                    table.ForeignKey(
                        name: "fk_parameter_overrides_parameters_parameter_id",
                        column: x => x.parameter_id,
                        principalSchema: "shared",
                        principalTable: "parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parameter_overrides_parameter_id",
                schema: "shared",
                table: "parameter_overrides",
                column: "parameter_id");

            migrationBuilder.CreateIndex(
                name: "ix_parameters_key",
                schema: "shared",
                table: "parameters",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameters_module_group_name",
                schema: "shared",
                table: "parameters",
                columns: new[] { "module", "group", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_overrides",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "parameters",
                schema: "shared");
        }
    }
}
