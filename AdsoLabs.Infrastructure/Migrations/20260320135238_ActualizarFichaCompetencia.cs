using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarFichaCompetencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.DropIndex(
                name: "IX_FC_Instructor_Fecha",
                schema: "adso",
                table: "FichaCompetencia");

            // Primero eliminar el CHECK viejo
            migrationBuilder.DropCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.DropColumn(
                name: "id_instructor",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "FichaCompetencia",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // Después actualizar los datos existentes
            migrationBuilder.Sql(
                "UPDATE [adso].[FichaCompetencia] SET [estado] = 'Programada' WHERE [estado] = 'En curso'");

            // Finalmente agregar el nuevo CHECK
            migrationBuilder.AddCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia",
                sql: "estado IN ('Pendiente','Programada','Finalizada')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "adso",
                table: "FichaCompetencia",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12);

            migrationBuilder.AddColumn<int>(
                name: "id_instructor",
                schema: "adso",
                table: "FichaCompetencia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FC_Instructor_Fecha",
                schema: "adso",
                table: "FichaCompetencia",
                columns: new[] { "id_instructor", "fecha_inicio", "fecha_fin" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FC_estado",
                schema: "adso",
                table: "FichaCompetencia",
                sql: "estado IN ('Pendiente','En curso','Finalizada')");

            migrationBuilder.AddForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia",
                column: "id_instructor",
                principalSchema: "adso",
                principalTable: "InstructorPerfil",
                principalColumn: "id_instructor",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
