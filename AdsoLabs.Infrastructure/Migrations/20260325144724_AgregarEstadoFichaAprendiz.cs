using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoFichaAprendiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "adso",
                table: "FichaAprendiz",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "En Formacion");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FichaAprendiz_estado",
                schema: "adso",
                table: "FichaAprendiz",
                sql: "estado IN ('En Formacion','Trasladado','Cancelado','Retiro Voluntario')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FichaAprendiz_estado",
                schema: "adso",
                table: "FichaAprendiz");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "adso",
                table: "FichaAprendiz");
        }
    }
}
