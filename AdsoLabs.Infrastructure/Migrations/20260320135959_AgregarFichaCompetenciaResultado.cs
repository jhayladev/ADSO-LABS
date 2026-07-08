using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFichaCompetenciaResultado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FichaCompetenciaResultado",
                schema: "adso",
                columns: table => new
                {
                    id_fcresultado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false),
                    id_resultado = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    horas_programadas = table.Column<int>(type: "int", nullable: true),
                    hora_inicio = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true),
                    hora_fin = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaCompetenciaResultado", x => x.id_fcresultado);
                    table.CheckConstraint("CK_FCR_estado", "estado IN ('Pendiente','Programado','Completado')");
                    table.ForeignKey(
                        name: "FK_FCR_FichaCompetencia",
                        column: x => x.id_ficha_competencia,
                        principalSchema: "adso",
                        principalTable: "FichaCompetencia",
                        principalColumn: "id_ficha_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FCR_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FCR_Resultado",
                        column: x => x.id_resultado,
                        principalSchema: "adso",
                        principalTable: "ResultadoAprendizaje",
                        principalColumn: "id_resultado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FCR_FichaCompetencia",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                column: "id_ficha_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_FCR_Instructor_Fecha",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                columns: new[] { "id_instructor", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_FCR_Resultado",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                column: "id_resultado");

            migrationBuilder.CreateIndex(
                name: "UX_FCR_FC_Resultado",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                columns: new[] { "id_ficha_competencia", "id_resultado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FichaCompetenciaResultado",
                schema: "adso");
        }
    }
}
