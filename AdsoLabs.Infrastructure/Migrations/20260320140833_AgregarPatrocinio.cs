using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPatrocinio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patrocinio",
                schema: "adso",
                columns: table => new
                {
                    id_patrocinio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    id_ficha = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    etapa = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    fecha_inicio_etapa = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_fin_etapa = table.Column<DateOnly>(type: "date", nullable: true),
                    hora_inicio = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true),
                    hora_fin = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true),
                    nombre_empresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    contacto_empresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patrocinio", x => x.id_patrocinio);
                    table.CheckConstraint("CK_Patrocinio_etapa", "etapa IN ('Lectiva','Productiva')");
                    table.ForeignKey(
                        name: "FK_Patrocinio_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patrocinio_Ficha",
                        column: x => x.id_ficha,
                        principalSchema: "adso",
                        principalTable: "Ficha",
                        principalColumn: "id_ficha",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patrocinio_Ficha",
                schema: "adso",
                table: "Patrocinio",
                column: "id_ficha");

            migrationBuilder.CreateIndex(
                name: "UX_Patrocinio_Aprendiz_Ficha_Activo",
                schema: "adso",
                table: "Patrocinio",
                columns: new[] { "id_aprendiz", "id_ficha" },
                unique: true,
                filter: "[activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patrocinio",
                schema: "adso");
        }
    }
}
