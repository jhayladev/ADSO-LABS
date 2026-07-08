using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil",
                sql: "estado IN ('En Formacion','Retiro Voluntario','Cancelado','Trasladado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Aprendiz_estado",
                schema: "adso",
                table: "AprendizPerfil",
                sql: "estado IN ('Activo','Retirado','Aplazado')");
        }
    }
}
