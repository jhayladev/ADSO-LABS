using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarCompetenciaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activa",
                schema: "adso",
                table: "Competencia");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "adso",
                table: "Competencia",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "Activa");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Competencia_estado",
                schema: "adso",
                table: "Competencia",
                sql: "estado IN ('Activa','Clausurada')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Competencia_estado",
                schema: "adso",
                table: "Competencia");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "adso",
                table: "Competencia");

            migrationBuilder.AddColumn<bool>(
                name: "activa",
                schema: "adso",
                table: "Competencia",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
