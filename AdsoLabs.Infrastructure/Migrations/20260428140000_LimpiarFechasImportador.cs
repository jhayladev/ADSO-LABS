using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LimpiarFechasImportador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Registros creados por el importador SOFIA antes de la corrección: tenían
            // fecha_inicio y fecha_fin rellenadas automáticamente. Ahora esos valores deben
            // ser NULL; la configuración manual en AsignacionCompetencias es quien los fija.
            migrationBuilder.Sql(
                "UPDATE adso.FichaCompetencia SET fecha_inicio = NULL, fecha_fin = NULL WHERE total_horas IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se puede restaurar los valores originales.
        }
    }
}
