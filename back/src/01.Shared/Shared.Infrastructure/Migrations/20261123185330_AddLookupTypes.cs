using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lookup_types",
                schema: "shared",
                columns: table => new
                {
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    allow_tenant_edit = table.Column<bool>(type: "boolean", nullable: false),
                    code_format = table.Column<byte>(type: "smallint", nullable: false),
                    code_max_length = table.Column<int>(type: "integer", nullable: false),
                    code_padding_width = table.Column<int>(type: "integer", nullable: true),
                    code_generation_mode = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lookup_types", x => x.type);
                    table.CheckConstraint("ck_lookup_types_code_format", "code_format IN (1, 2)");
                    table.CheckConstraint("ck_lookup_types_code_generation_mode", "code_generation_mode IN (1, 2, 3)");
                    table.CheckConstraint("ck_lookup_types_code_max_length", "code_max_length > 0 AND code_max_length <= 100");
                    table.CheckConstraint("ck_lookup_types_code_padding_width", "code_padding_width IS NULL OR (code_padding_width > 0 AND code_padding_width <= code_max_length)");
                    table.CheckConstraint("ck_lookup_types_type", "type <> '' AND type = lower(btrim(type))");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lookup_types",
                schema: "shared");
        }
    }
}
