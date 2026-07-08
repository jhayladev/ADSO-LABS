using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAcudienteYObservaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acudiente",
                schema: "adso",
                columns: table => new
                {
                    id_acudiente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    parentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    genero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    tipo_documento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    numero_documento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    estrato = table.Column<byte>(type: "tinyint", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acudiente", x => x.id_acudiente);
                    table.ForeignKey(
                        name: "FK_Acudiente_AprendizPerfil",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObservacionAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_observacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservacionAprendiz", x => x.id_observacion);
                    table.ForeignKey(
                        name: "FK_ObservacionAprendiz_AprendizPerfil",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObservacionAprendiz_Usuario",
                        column: x => x.id_usuario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acudiente_id_aprendiz",
                schema: "adso",
                table: "Acudiente",
                column: "id_aprendiz",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_aprendiz_fecha",
                schema: "adso",
                table: "ObservacionAprendiz",
                columns: new[] { "id_aprendiz", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_ObservacionAprendiz_id_usuario",
                schema: "adso",
                table: "ObservacionAprendiz",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acudiente",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ObservacionAprendiz",
                schema: "adso");
        }
    }
}
