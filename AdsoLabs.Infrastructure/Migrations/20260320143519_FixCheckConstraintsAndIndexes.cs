using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCheckConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Juicio_juicio",
                schema: "adso",
                table: "JuicioResultado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FCR_estado",
                schema: "adso",
                table: "FichaCompetenciaResultado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ficha_estado",
                schema: "adso",
                table: "Ficha");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_id_persona",
                schema: "adso",
                table: "Usuario",
                newName: "UX_Usuario_Persona");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_Correo",
                schema: "adso",
                table: "Usuario",
                newName: "UX_Usuario_Correo");

            migrationBuilder.RenameIndex(
                name: "IX_Persona_Documento",
                schema: "adso",
                table: "Persona",
                newName: "UX_Persona_Documento");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "AprendizPerfil",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "En Formacion",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Activo");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Juicio_juicio",
                schema: "adso",
                table: "JuicioResultado",
                sql: "juicio IN ('Aprobado','Por Evaluar')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FCR_estado",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                sql: "estado IN ('Pendiente','Programado','En Curso','Completado')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ficha_estado",
                schema: "adso",
                table: "Ficha",
                sql: "estado IN ('Activa','Finalizada', 'Suspendida')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Juicio_juicio",
                schema: "adso",
                table: "JuicioResultado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FCR_estado",
                schema: "adso",
                table: "FichaCompetenciaResultado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ficha_estado",
                schema: "adso",
                table: "Ficha");

            migrationBuilder.RenameIndex(
                name: "UX_Usuario_Persona",
                schema: "adso",
                table: "Usuario",
                newName: "IX_Usuario_id_persona");

            migrationBuilder.RenameIndex(
                name: "UX_Usuario_Correo",
                schema: "adso",
                table: "Usuario",
                newName: "IX_Usuario_Correo");

            migrationBuilder.RenameIndex(
                name: "UX_Persona_Documento",
                schema: "adso",
                table: "Persona",
                newName: "IX_Persona_Documento");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "AprendizPerfil",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Activo",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "En Formacion");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Juicio_juicio",
                schema: "adso",
                table: "JuicioResultado",
                sql: "juicio IN ('Aprobado','No Aprobado','Por Evaluar')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FCR_estado",
                schema: "adso",
                table: "FichaCompetenciaResultado",
                sql: "estado IN ('Pendiente','Programado','Completado')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ficha_estado",
                schema: "adso",
                table: "Ficha",
                sql: "estado IN ('Activa','Finalizada','Suspendida')");
        }
    }
}
