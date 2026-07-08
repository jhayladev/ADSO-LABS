using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudAsignacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudAsignacion",
                schema: "adso",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Pendiente"),
                    razonamiento_ia = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    porcentaje_compatibilidad = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_respuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    observacion_admin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudAsignacion", x => x.id_solicitud);
                    table.CheckConstraint("CK_SA_estado", "estado IN ('Pendiente','Aprobada','Rechazada')");
                    table.ForeignKey(
                        name: "FK_SA_FichaCompetencia",
                        column: x => x.id_ficha_competencia,
                        principalSchema: "adso",
                        principalTable: "FichaCompetencia",
                        principalColumn: "id_ficha_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SA_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SA_FichaCompetencia",
                schema: "adso",
                table: "SolicitudAsignacion",
                column: "id_ficha_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_SA_Instructor",
                schema: "adso",
                table: "SolicitudAsignacion",
                column: "id_instructor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudAsignacion",
                schema: "adso");
        }
    }
}
