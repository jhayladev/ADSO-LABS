using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InstructorNullable_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Instructor",
                schema: "adso",
                table: "PlanConcertado");

            migrationBuilder.AlterColumn<int>(
                name: "id_instructor",
                schema: "adso",
                table: "PlanConcertado",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "id_instructor",
                schema: "adso",
                table: "FichaCompetencia",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia",
                column: "id_instructor",
                principalSchema: "adso",
                principalTable: "InstructorPerfil",
                principalColumn: "id_instructor",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Instructor",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_instructor",
                principalSchema: "adso",
                principalTable: "InstructorPerfil",
                principalColumn: "id_instructor",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Plan_Instructor",
                schema: "adso",
                table: "PlanConcertado");

            migrationBuilder.AlterColumn<int>(
                name: "id_instructor",
                schema: "adso",
                table: "PlanConcertado",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_instructor",
                schema: "adso",
                table: "FichaCompetencia",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FC_Instructor",
                schema: "adso",
                table: "FichaCompetencia",
                column: "id_instructor",
                principalSchema: "adso",
                principalTable: "InstructorPerfil",
                principalColumn: "id_instructor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_Instructor",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_instructor",
                principalSchema: "adso",
                principalTable: "InstructorPerfil",
                principalColumn: "id_instructor",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
