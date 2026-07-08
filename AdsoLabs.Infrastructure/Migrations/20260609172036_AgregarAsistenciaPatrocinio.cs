using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAsistenciaPatrocinio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsistenciaPatrocinio",
                schema: "adso",
                columns: table => new
                {
                    id_asistencia_patrocinio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_patrocinio = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistenciaPatrocinio", x => x.id_asistencia_patrocinio);
                    table.CheckConstraint("CK_AsistenciaPatrocinio_estado", "estado IN ('Presente','Ausente','Justificado')");
                    table.ForeignKey(
                        name: "FK_AsistenciaPatrocinio_Patrocinio",
                        column: x => x.id_patrocinio,
                        principalSchema: "adso",
                        principalTable: "Patrocinio",
                        principalColumn: "id_patrocinio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciaPatrocinio_Fecha",
                schema: "adso",
                table: "AsistenciaPatrocinio",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "UX_AsistenciaPatrocinio_Patrocinio_Fecha",
                schema: "adso",
                table: "AsistenciaPatrocinio",
                columns: new[] { "id_patrocinio", "fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsistenciaPatrocinio",
                schema: "adso");
        }
    }
}
