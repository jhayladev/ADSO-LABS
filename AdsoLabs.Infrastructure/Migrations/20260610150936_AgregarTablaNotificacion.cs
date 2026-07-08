using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacion",
                schema: "adso",
                columns: table => new
                {
                    id_notificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario_destinatario = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    fecha_lectura = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    referencia_id = table.Column<int>(type: "int", nullable: true),
                    referencia_tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK_Notificacion_Usuario",
                        column: x => x.id_usuario_destinatario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_id_usuario_destinatario_leida_fecha_creacion",
                schema: "adso",
                table: "Notificacion",
                columns: new[] { "id_usuario_destinatario", "leida", "fecha_creacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacion",
                schema: "adso");
        }
    }
}
