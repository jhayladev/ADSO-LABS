using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarEstadoAprendizPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrar el estado global al estado por ficha en FichaAprendiz
            migrationBuilder.Sql(@"
                UPDATE fa
                SET fa.estado = ap.estado
                FROM adso.FichaAprendiz fa
                INNER JOIN adso.AprendizPerfil ap ON ap.id_aprendiz = fa.id_aprendiz
            ");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "adso",
                table: "AprendizPerfil");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "adso",
                table: "AprendizPerfil",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "En Formacion");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil",
                sql: "estado IN ('En Formacion','Retiro Voluntario','Cancelado','Trasladado')");
        }
    }
}
