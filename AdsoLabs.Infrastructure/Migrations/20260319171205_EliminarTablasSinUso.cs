using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarTablasSinUso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_Sesion",
                schema: "adso",
                table: "ImportacionArchivo");

            migrationBuilder.DropTable(
                name: "ConfiguracionAlerta",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Notificacion",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ObservacionAprendiz",
                schema: "adso");

            migrationBuilder.RenameColumn(
                name: "id_sesion_destino",
                schema: "adso",
                table: "ImportacionArchivo",
                newName: "SesionDestinoIdSesion");

            migrationBuilder.RenameIndex(
                name: "IX_ImportacionArchivo_id_sesion_destino",
                schema: "adso",
                table: "ImportacionArchivo",
                newName: "IX_ImportacionArchivo_SesionDestinoIdSesion");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportacionArchivo_Sesion_SesionDestinoIdSesion",
                schema: "adso",
                table: "ImportacionArchivo",
                column: "SesionDestinoIdSesion",
                principalSchema: "adso",
                principalTable: "Sesion",
                principalColumn: "id_sesion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportacionArchivo_Sesion_SesionDestinoIdSesion",
                schema: "adso",
                table: "ImportacionArchivo");

            migrationBuilder.RenameColumn(
                name: "SesionDestinoIdSesion",
                schema: "adso",
                table: "ImportacionArchivo",
                newName: "id_sesion_destino");

            migrationBuilder.RenameIndex(
                name: "IX_ImportacionArchivo_SesionDestinoIdSesion",
                schema: "adso",
                table: "ImportacionArchivo",
                newName: "IX_ImportacionArchivo_id_sesion_destino");

            migrationBuilder.CreateTable(
                name: "ConfiguracionAlerta",
                schema: "adso",
                columns: table => new
                {
                    id_config = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    canal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    plantilla_asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    plantilla_cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo_alerta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    umbral = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionAlerta", x => x.id_config);
                    table.CheckConstraint("CK_CfgAlerta_canal", "canal IN ('email','app','ambos')");
                    table.ForeignKey(
                        name: "FK_CfgAlerta_Usuario",
                        column: x => x.id_usuario_modifico,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificacion",
                schema: "adso",
                columns: table => new
                {
                    id_notificacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_destinatario_usuario = table.Column<int>(type: "int", nullable: true),
                    asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    canal_usado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    fecha_generacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    leido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    tipo_alerta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.id_notificacion);
                    table.CheckConstraint("CK_Noti_estado", "estado IN ('Pendiente','Enviada','Error')");
                    table.ForeignKey(
                        name: "FK_Noti_DestUsuario",
                        column: x => x.id_destinatario_usuario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ObservacionAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_observacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    id_competencia = table.Column<int>(type: "int", nullable: true),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_sesion = table.Column<int>(type: "int", nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservacionAprendiz", x => x.id_observacion);
                    table.CheckConstraint("CK_Obs_tipo", "tipo IN ('General','Academica','Convivencia','Seguimiento')");
                    table.ForeignKey(
                        name: "FK_Obs_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Obs_Competencia",
                        column: x => x.id_competencia,
                        principalSchema: "adso",
                        principalTable: "Competencia",
                        principalColumn: "id_competencia",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Obs_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Obs_Sesion",
                        column: x => x.id_sesion,
                        principalSchema: "adso",
                        principalTable: "Sesion",
                        principalColumn: "id_sesion",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionAlerta_id_usuario_modifico",
                schema: "adso",
                table: "ConfiguracionAlerta",
                column: "id_usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "UX_CfgAlerta_tipo",
                schema: "adso",
                table: "ConfiguracionAlerta",
                column: "tipo_alerta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noti_Estado_Fecha",
                schema: "adso",
                table: "Notificacion",
                columns: new[] { "estado", "fecha_generacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_id_destinatario_usuario",
                schema: "adso",
                table: "Notificacion",
                column: "id_destinatario_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_aprendiz",
                schema: "adso",
                table: "ObservacionAprendiz",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_competencia",
                schema: "adso",
                table: "ObservacionAprendiz",
                column: "id_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_instructor",
                schema: "adso",
                table: "ObservacionAprendiz",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_sesion",
                schema: "adso",
                table: "ObservacionAprendiz",
                column: "id_sesion");

            migrationBuilder.AddForeignKey(
                name: "FK_Import_Sesion",
                schema: "adso",
                table: "ImportacionArchivo",
                column: "id_sesion_destino",
                principalSchema: "adso",
                principalTable: "Sesion",
                principalColumn: "id_sesion",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
