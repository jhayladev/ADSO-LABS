using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialEstadoAprendiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialEstadoAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_historial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    estado_anterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estado_nuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    id_usuario_cambio = table.Column<int>(type: "int", nullable: true),
                    fecha_cambio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoAprendiz", x => x.id_historial);
                    table.ForeignKey(
                        name: "FK_HistorialEstado_FichaAprendiz",
                        columns: x => new { x.id_ficha, x.id_aprendiz },
                        principalSchema: "adso",
                        principalTable: "FichaAprendiz",
                        principalColumns: new[] { "id_ficha", "id_aprendiz" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstado_FichaAprendiz",
                schema: "adso",
                table: "HistorialEstadoAprendiz",
                columns: new[] { "id_ficha", "id_aprendiz" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialEstadoAprendiz",
                schema: "adso");
        }
    }
}
