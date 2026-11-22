using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations;

/// <inheritdoc />
public partial class _20261121013113_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Producto",
            columns: table => new
            {
                ID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nombre = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Producto", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Proveedor",
            columns: table => new
            {
                ID = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nombre = table.Column<string>(type: "text", nullable: false),
                Direccion = table.Column<string>(type: "text", nullable: false),
                RUC = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Proveedor", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Lote",
            columns: table => new
            {
                IDProducto = table.Column<int>(type: "integer", nullable: false),
                IDProveedor = table.Column<int>(type: "integer", nullable: false),
                Precio = table.Column<int>(type: "integer", nullable: false),
                Stock = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Lote", x => new { x.IDProducto, x.IDProveedor });
                table.ForeignKey(
                    name: "FK_Lote_Producto_IDProducto",
                    column: x => x.IDProducto,
                    principalTable: "Producto",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Lote_Proveedor_IDProveedor",
                    column: x => x.IDProveedor,
                    principalTable: "Proveedor",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Lote_IDProveedor",
            table: "Lote",
            column: "IDProveedor");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Lote");

        migrationBuilder.DropTable(
            name: "Producto");

        migrationBuilder.DropTable(
            name: "Proveedor");
    }
}
