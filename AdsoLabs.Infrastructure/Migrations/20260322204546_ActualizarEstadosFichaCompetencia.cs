using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEstadosFichaCompetencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia");

            // Migrar datos existentes al nuevo vocabulario del SRS
            migrationBuilder.Sql(
                "UPDATE adso.\"FichaCompetencia\" SET estado = 'Vista' WHERE estado = 'Finalizada'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia",
                sql: "estado IN ('Pendiente','En Curso','Programada','Vista')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.Sql(
                "UPDATE adso.\"FichaCompetencia\" SET estado = 'Finalizada' WHERE estado = 'Vista'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia",
                sql: "estado IN ('Pendiente','Programada','Finalizada')");
        }
    }
}
