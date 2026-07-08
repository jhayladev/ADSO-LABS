using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarAsistenciaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia");

            migrationBuilder.DropColumn(
                name: "justificado",
                schema: "adso",
                table: "Asistencia");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia",
                sql: "estado IN ('Presente','Ausente','Justificado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia");

            migrationBuilder.AddColumn<bool>(
                name: "justificado",
                schema: "adso",
                table: "Asistencia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia",
                sql: "estado IN ('Presente','Ausente')");
        }
    }
}
