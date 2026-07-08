using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoSesionYTarde : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "adso",
                table: "Sesion",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Abierta");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "Asistencia",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sesion_estado",
                schema: "adso",
                table: "Sesion",
                sql: "estado IN ('Abierta','Finalizada','Cancelada')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia",
                sql: "estado IN ('Presente','Ausente','Justificado','Tarde')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sesion_estado",
                schema: "adso",
                table: "Sesion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "adso",
                table: "Sesion");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "Asistencia",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Asistencia_estado",
                schema: "adso",
                table: "Asistencia",
                sql: "estado IN ('Presente','Ausente','Justificado')");
        }
    }
}
