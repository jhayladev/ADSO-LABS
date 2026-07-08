using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSesionIdFichaCompetenciaResultado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las sesiones existentes fueron creadas sin IdFichaCompetenciaResultado
            // (bug: estaban ligadas solo a FichaCompetencia y eran compartidas entre resultados).
            // Se eliminan junto con sus registros de asistencia para poder agregar el FK limpiamente.
            migrationBuilder.Sql("DELETE FROM [adso].[Asistencia]");
            migrationBuilder.Sql("DELETE FROM [adso].[Sesion]");

            migrationBuilder.AddColumn<int>(
                name: "id_ficha_competencia_resultado",
                schema: "adso",
                table: "Sesion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sesion_id_ficha_competencia_resultado",
                schema: "adso",
                table: "Sesion",
                column: "id_ficha_competencia_resultado");

            migrationBuilder.AddForeignKey(
                name: "FK_Sesion_FCR",
                schema: "adso",
                table: "Sesion",
                column: "id_ficha_competencia_resultado",
                principalSchema: "adso",
                principalTable: "FichaCompetenciaResultado",
                principalColumn: "id_fcresultado",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sesion_FCR",
                schema: "adso",
                table: "Sesion");

            migrationBuilder.DropIndex(
                name: "IX_Sesion_id_ficha_competencia_resultado",
                schema: "adso",
                table: "Sesion");

            migrationBuilder.DropColumn(
                name: "id_ficha_competencia_resultado",
                schema: "adso",
                table: "Sesion");
        }
    }
}
