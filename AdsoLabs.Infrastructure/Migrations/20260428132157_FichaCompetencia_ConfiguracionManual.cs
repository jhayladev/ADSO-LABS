using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FichaCompetencia_ConfiguracionManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "fecha_inicio",
                schema: "adso",
                table: "FichaCompetencia",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "total_horas",
                schema: "adso",
                table: "FichaCompetencia",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_horas",
                schema: "adso",
                table: "FichaCompetencia");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "fecha_inicio",
                schema: "adso",
                table: "FichaCompetencia",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
