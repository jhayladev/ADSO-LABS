using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlanConcertadoDocumentoNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Archivo",
                schema: "adso",
                table: "PlanConcertado");

            migrationBuilder.AlterColumn<int>(
                name: "id_documento",
                schema: "adso",
                table: "PlanConcertado",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Archivo",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_documento",
                principalSchema: "adso",
                principalTable: "Archivo",
                principalColumn: "id_archivo",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Archivo",
                schema: "adso",
                table: "PlanConcertado");

            migrationBuilder.AlterColumn<int>(
                name: "id_documento",
                schema: "adso",
                table: "PlanConcertado",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Archivo",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_documento",
                principalSchema: "adso",
                principalTable: "Archivo",
                principalColumn: "id_archivo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
