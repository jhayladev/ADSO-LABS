using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloMonitorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitorPerfil",
                schema: "adso",
                columns: table => new
                {
                    id_monitor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fecha_asignacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorPerfil", x => x.id_monitor);
                    table.ForeignKey(
                        name: "FK_MonitorPerfil_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SesionMonitoria",
                schema: "adso",
                columns: table => new
                {
                    id_sesion_monitoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_monitor = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    jornada = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    modalidad = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Activa"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionMonitoria", x => x.id_sesion_monitoria);
                    table.CheckConstraint("CK_SesionMonitoria_estado", "estado IN ('Activa','Finalizada','Cancelada')");
                    table.CheckConstraint("CK_SesionMonitoria_jornada", "jornada IN ('Mañana','Tarde')");
                    table.CheckConstraint("CK_SesionMonitoria_modalidad", "modalidad IN ('Presencial','Virtual')");
                    table.ForeignKey(
                        name: "FK_SesionMonitoria_Monitor",
                        column: x => x.id_monitor,
                        principalSchema: "adso",
                        principalTable: "MonitorPerfil",
                        principalColumn: "id_monitor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsistenciaMonitoria",
                schema: "adso",
                columns: table => new
                {
                    id_asistencia_monitoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sesion_monitoria = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    id_instructor_destinatario = table.Column<int>(type: "int", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistenciaMonitoria", x => x.id_asistencia_monitoria);
                    table.CheckConstraint("CK_AsistenciaMonitoria_estado", "estado IN ('Presente','Justificado','Ausente')");
                    table.ForeignKey(
                        name: "FK_AsistenciaMonitoria_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsistenciaMonitoria_Instructor",
                        column: x => x.id_instructor_destinatario,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsistenciaMonitoria_Sesion",
                        column: x => x.id_sesion_monitoria,
                        principalSchema: "adso",
                        principalTable: "SesionMonitoria",
                        principalColumn: "id_sesion_monitoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InformeMonitoria",
                schema: "adso",
                columns: table => new
                {
                    id_informe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sesion_monitoria = table.Column<int>(type: "int", nullable: false),
                    id_instructor_destinatario = table.Column<int>(type: "int", nullable: false),
                    texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformeMonitoria", x => x.id_informe);
                    table.ForeignKey(
                        name: "FK_InformeMonitoria_Instructor",
                        column: x => x.id_instructor_destinatario,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InformeMonitoria_Sesion",
                        column: x => x.id_sesion_monitoria,
                        principalSchema: "adso",
                        principalTable: "SesionMonitoria",
                        principalColumn: "id_sesion_monitoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InscripcionMonitoria",
                schema: "adso",
                columns: table => new
                {
                    id_inscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sesion_monitoria = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    fecha_inscripcion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionMonitoria", x => x.id_inscripcion);
                    table.ForeignKey(
                        name: "FK_InscripcionMonitoria_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionMonitoria_Sesion",
                        column: x => x.id_sesion_monitoria,
                        principalSchema: "adso",
                        principalTable: "SesionMonitoria",
                        principalColumn: "id_sesion_monitoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InformeMonitoriaAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_informe = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformeMonitoriaAprendiz", x => new { x.id_informe, x.id_aprendiz });
                    table.ForeignKey(
                        name: "FK_InformeMonitoriaAprendiz_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InformeMonitoriaAprendiz_Informe",
                        column: x => x.id_informe,
                        principalSchema: "adso",
                        principalTable: "InformeMonitoria",
                        principalColumn: "id_informe",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciaMonitoria_id_aprendiz",
                schema: "adso",
                table: "AsistenciaMonitoria",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciaMonitoria_id_instructor_destinatario",
                schema: "adso",
                table: "AsistenciaMonitoria",
                column: "id_instructor_destinatario");

            migrationBuilder.CreateIndex(
                name: "UX_AsistenciaMonitoria_Sesion_Aprendiz_Fecha",
                schema: "adso",
                table: "AsistenciaMonitoria",
                columns: new[] { "id_sesion_monitoria", "id_aprendiz", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InformeMonitoria_id_instructor_destinatario",
                schema: "adso",
                table: "InformeMonitoria",
                column: "id_instructor_destinatario");

            migrationBuilder.CreateIndex(
                name: "IX_InformeMonitoria_Sesion",
                schema: "adso",
                table: "InformeMonitoria",
                column: "id_sesion_monitoria");

            migrationBuilder.CreateIndex(
                name: "IX_InformeMonitoriaAprendiz_id_aprendiz",
                schema: "adso",
                table: "InformeMonitoriaAprendiz",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionMonitoria_id_aprendiz",
                schema: "adso",
                table: "InscripcionMonitoria",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "UX_InscripcionMonitoria_Sesion_Aprendiz",
                schema: "adso",
                table: "InscripcionMonitoria",
                columns: new[] { "id_sesion_monitoria", "id_aprendiz" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MonitorPerfil_Aprendiz_Activo",
                schema: "adso",
                table: "MonitorPerfil",
                column: "id_aprendiz",
                unique: true,
                filter: "[activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SesionMonitoria_Monitor",
                schema: "adso",
                table: "SesionMonitoria",
                column: "id_monitor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsistenciaMonitoria",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "InformeMonitoriaAprendiz",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "InscripcionMonitoria",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "InformeMonitoria",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "SesionMonitoria",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "MonitorPerfil",
                schema: "adso");
        }
    }
}
