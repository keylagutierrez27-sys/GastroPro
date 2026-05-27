using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GastroPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CierresCaja",
                columns: table => new
                {
                    CierreCajaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalVendido = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresCaja", x => x.CierreCajaId);
                });

            migrationBuilder.CreateTable(
                name: "Platos",
                columns: table => new
                {
                    PlatoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platos", x => x.PlatoId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contrasena = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroMesa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NroOperacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CierreCajaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.ForeignKey(
                        name: "FK_Pagos_CierresCaja_CierreCajaId",
                        column: x => x.CierreCajaId,
                        principalTable: "CierresCaja",
                        principalColumn: "CierreCajaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    PedidoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroMesa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.PedidoId);
                    table.ForeignKey(
                        name: "FK_Pedidos_Platos_PlatoId",
                        column: x => x.PlatoId,
                        principalTable: "Platos",
                        principalColumn: "PlatoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Platos",
                columns: new[] { "PlatoId", "Categoria", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, "Caldos", "Caldo de Gallina", 15.00m },
                    { 2, "Menu", "Arroz Chaufa", 12.00m },
                    { 3, "Segundos", "Lomo Saltado", 18.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CierreCajaId",
                table: "Pagos",
                column: "CierreCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_PlatoId",
                table: "Pedidos",
                column: "PlatoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "CierresCaja");

            migrationBuilder.DropTable(
                name: "Platos");
        }
    }
}
