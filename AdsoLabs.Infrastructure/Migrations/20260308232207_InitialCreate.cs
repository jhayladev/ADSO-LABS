using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdsoLabs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adso");

            migrationBuilder.CreateTable(
                name: "Auditoria",
                schema: "adso",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: true),
                    accion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tabla_afectada = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    id_registro_afectado = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    datos_viejos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datos_nuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    direccion_ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.id_auditoria);
                });

            migrationBuilder.CreateTable(
                name: "Competencia",
                schema: "adso",
                columns: table => new
                {
                    id_competencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horas_asignadas = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Tecnica"),
                    activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencia", x => x.id_competencia);
                    table.CheckConstraint("CK_Competencia_horas", "horas_asignadas > 0");
                    table.CheckConstraint("CK_Competencia_tipo", "tipo IN ('Tecnica','Transversal','Clave','Induccion','Practica')");
                });

            migrationBuilder.CreateTable(
                name: "Ficha",
                schema: "adso",
                columns: table => new
                {
                    id_ficha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    numero_ficha = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ficha", x => x.id_ficha);
                    table.CheckConstraint("CK_Ficha_estado", "estado IN ('Activa','Finalizada','Suspendida')");
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                schema: "adso",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "TipoArchivo",
                schema: "adso",
                columns: table => new
                {
                    id_tipo_archivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoArchivo", x => x.id_tipo_archivo);
                });

            migrationBuilder.CreateTable(
                name: "TipoDocumento",
                schema: "adso",
                columns: table => new
                {
                    id_tipo_documento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDocumento", x => x.id_tipo_documento);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                schema: "adso",
                columns: table => new
                {
                    id_persona = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_tipo_documento = table.Column<int>(type: "int", nullable: false),
                    numero_documento = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.id_persona);
                    table.ForeignKey(
                        name: "FK_Persona_TipoDoc",
                        column: x => x.id_tipo_documento,
                        principalSchema: "adso",
                        principalTable: "TipoDocumento",
                        principalColumn: "id_tipo_documento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "adso",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    id_rol = table.Column<int>(type: "int", nullable: false),
                    correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: true),
                    password_salt = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    primer_login = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    intentos_fallidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    bloqueado_hasta = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    fecha_inicio_sesion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona",
                        column: x => x.id_persona,
                        principalSchema: "adso",
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol",
                        column: x => x.id_rol,
                        principalSchema: "adso",
                        principalTable: "Rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AprendizPerfil",
                schema: "adso",
                columns: table => new
                {
                    id_aprendiz = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: true),
                    estrato = table.Column<byte>(type: "tinyint", nullable: true),
                    condicion_especial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    tipo_poblacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    contacto_emergencia_nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    contacto_emergencia_telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AprendizPerfil", x => x.id_aprendiz);
                    table.CheckConstraint("CK_Aprendiz_estado", "estado IN ('Activo','Retirado','Aplazado')");
                    table.CheckConstraint("CK_Aprendiz_estrato", "estrato BETWEEN 0 AND 6");
                    table.ForeignKey(
                        name: "FK_AprendizPerfil_Persona",
                        column: x => x.id_persona,
                        principalSchema: "adso",
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AprendizPerfil_Usuario",
                        column: x => x.id_usuario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Archivo",
                schema: "adso",
                columns: table => new
                {
                    id_archivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_original = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    nombre_almacenado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ruta_storage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    extension_archivo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    id_tipo_archivo = table.Column<int>(type: "int", nullable: false),
                    id_usuario_subio = table.Column<int>(type: "int", nullable: false),
                    tamanio_bytes = table.Column<long>(type: "bigint", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    site_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    drive_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    item_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    web_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_subida = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivo", x => x.id_archivo);
                    table.ForeignKey(
                        name: "FK_Archivo_SubidoPor",
                        column: x => x.id_usuario_subio,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Archivo_Tipo",
                        column: x => x.id_tipo_archivo,
                        principalSchema: "adso",
                        principalTable: "TipoArchivo",
                        principalColumn: "id_tipo_archivo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionAlerta",
                schema: "adso",
                columns: table => new
                {
                    id_config = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_alerta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    umbral = table.Column<int>(type: "int", nullable: false),
                    canal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    plantilla_asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    plantilla_cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    id_usuario_modifico = table.Column<int>(type: "int", nullable: false)
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
                name: "InstructorPerfil",
                schema: "adso",
                columns: table => new
                {
                    id_instructor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    numero_contrato = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    especialidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    tipo_vinculacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    fecha_vinculacion = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorPerfil", x => x.id_instructor);
                    table.CheckConstraint("CK_InstrPerfil_vinculacion", "tipo_vinculacion IN ('Planta','Contratista','Provisional')");
                    table.ForeignKey(
                        name: "FK_InstrPerfil_Usuario",
                        column: x => x.id_usuario,
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
                    tipo_alerta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    id_destinatario_usuario = table.Column<int>(type: "int", nullable: true),
                    asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    canal_usado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    fecha_generacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    fecha_envio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    leido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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
                name: "PasswordResetToken",
                schema: "adso",
                columns: table => new
                {
                    id_token = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    fecha_expiracion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    fecha_uso = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.id_token);
                    table.ForeignKey(
                        name: "FK_PRT_Usuario",
                        column: x => x.id_usuario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_ficha = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaAprendiz", x => new { x.id_ficha, x.id_aprendiz });
                    table.ForeignKey(
                        name: "FK_FichaAprendiz_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FichaAprendiz_Ficha",
                        column: x => x.id_ficha,
                        principalSchema: "adso",
                        principalTable: "Ficha",
                        principalColumn: "id_ficha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesarrolloCurricular",
                schema: "adso",
                columns: table => new
                {
                    id_desarrollo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesarrolloCurricular", x => x.id_desarrollo);
                    table.CheckConstraint("CK_DesaCurr_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_DesaCurr_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesaCurr_Competencia",
                        column: x => x.id_competencia,
                        principalSchema: "adso",
                        principalTable: "Competencia",
                        principalColumn: "id_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesaCurr_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FichaCompetencia",
                schema: "adso",
                columns: table => new
                {
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha = table.Column<int>(type: "int", nullable: false),
                    id_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    horas_ejecutadas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaCompetencia", x => x.id_ficha_competencia);
                    table.CheckConstraint("CK_FC_estado", "estado IN ('Pendiente','En curso','Finalizada')");
                    table.ForeignKey(
                        name: "FK_FC_Competencia",
                        column: x => x.id_competencia,
                        principalSchema: "adso",
                        principalTable: "Competencia",
                        principalColumn: "id_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FC_Ficha",
                        column: x => x.id_ficha,
                        principalSchema: "adso",
                        principalTable: "Ficha",
                        principalColumn: "id_ficha",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FC_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentoEvaluacion",
                schema: "adso",
                columns: table => new
                {
                    id_instrumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentoEvaluacion", x => x.id_instrumento);
                    table.CheckConstraint("CK_Inst_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_Inst_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inst_Competencia",
                        column: x => x.id_competencia,
                        principalSchema: "adso",
                        principalTable: "Competencia",
                        principalColumn: "id_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inst_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanConcertado",
                schema: "adso",
                columns: table => new
                {
                    id_plan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    descripcion_general = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Plan de evaluacion concertado con los aprendices de acuerdo con los resultados de aprendizaje de la competencia."),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_cierre = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanConcertado", x => x.id_plan);
                    table.CheckConstraint("CK_Plan_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_Plan_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plan_Competencia",
                        column: x => x.id_competencia,
                        principalSchema: "adso",
                        principalTable: "Competencia",
                        principalColumn: "id_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plan_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProyectoFormativo",
                schema: "adso",
                columns: table => new
                {
                    id_proyecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoFormativo", x => x.id_proyecto);
                    table.CheckConstraint("CK_Proy_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_Proy_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proy_Ficha",
                        column: x => x.id_ficha,
                        principalSchema: "adso",
                        principalTable: "Ficha",
                        principalColumn: "id_ficha",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proy_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuiaAprendizaje",
                schema: "adso",
                columns: table => new
                {
                    id_guia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuiaAprendizaje", x => x.id_guia);
                    table.CheckConstraint("CK_Guia_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_Guia_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guia_FC",
                        column: x => x.id_ficha_competencia,
                        principalSchema: "adso",
                        principalTable: "FichaCompetencia",
                        principalColumn: "id_ficha_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guia_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaneacionPedagogica",
                schema: "adso",
                columns: table => new
                {
                    id_planeacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Borrador"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaneacionPedagogica", x => x.id_planeacion);
                    table.CheckConstraint("CK_Planea_estado", "estado IN ('Borrador','Publicado','Cerrado')");
                    table.ForeignKey(
                        name: "FK_Planea_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Planea_FC",
                        column: x => x.id_ficha_competencia,
                        principalSchema: "adso",
                        principalTable: "FichaCompetencia",
                        principalColumn: "id_ficha_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Planea_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sesion",
                schema: "adso",
                columns: table => new
                {
                    id_sesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_ficha_competencia = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sesion", x => x.id_sesion);
                    table.ForeignKey(
                        name: "FK_Sesion_FC",
                        column: x => x.id_ficha_competencia,
                        principalSchema: "adso",
                        principalTable: "FichaCompetencia",
                        principalColumn: "id_ficha_competencia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sesion_Instructor",
                        column: x => x.id_instructor,
                        principalSchema: "adso",
                        principalTable: "InstructorPerfil",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultadoAprendizaje",
                schema: "adso",
                columns: table => new
                {
                    id_resultado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_plan = table.Column<int>(type: "int", nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoAprendizaje", x => x.id_resultado);
                    table.ForeignKey(
                        name: "FK_Res_Plan",
                        column: x => x.id_plan,
                        principalSchema: "adso",
                        principalTable: "PlanConcertado",
                        principalColumn: "id_plan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asistencia",
                schema: "adso",
                columns: table => new
                {
                    id_asistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sesion = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    justificado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    motivo_inasistencia = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.id_asistencia);
                    table.CheckConstraint("CK_Asistencia_estado", "estado IN ('Presente','Ausente')");
                    table.ForeignKey(
                        name: "FK_Asistencia_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencia_Sesion",
                        column: x => x.id_sesion,
                        principalSchema: "adso",
                        principalTable: "Sesion",
                        principalColumn: "id_sesion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportacionArchivo",
                schema: "adso",
                columns: table => new
                {
                    id_importacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_entidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    id_documento = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_sesion_destino = table.Column<int>(type: "int", nullable: true),
                    estado_procesamiento = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Pendiente"),
                    total_filas = table.Column<int>(type: "int", nullable: true),
                    filas_ok = table.Column<int>(type: "int", nullable: true),
                    filas_error = table.Column<int>(type: "int", nullable: true),
                    observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_subida = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()"),
                    fecha_procesado = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportacionArchivo", x => x.id_importacion);
                    table.CheckConstraint("CK_Import_estado", "estado_procesamiento IN ('Pendiente','Procesado','Error')");
                    table.CheckConstraint("CK_Import_tipo", "tipo_entidad IN ('Aprendiz','Instructor','Ficha','Competencia','FichaCompetencia','Asistencia')");
                    table.ForeignKey(
                        name: "FK_Import_Archivo",
                        column: x => x.id_documento,
                        principalSchema: "adso",
                        principalTable: "Archivo",
                        principalColumn: "id_archivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Import_Sesion",
                        column: x => x.id_sesion_destino,
                        principalSchema: "adso",
                        principalTable: "Sesion",
                        principalColumn: "id_sesion",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Import_Usuario",
                        column: x => x.id_usuario,
                        principalSchema: "adso",
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ObservacionAprendiz",
                schema: "adso",
                columns: table => new
                {
                    id_observacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    id_competencia = table.Column<int>(type: "int", nullable: true),
                    id_sesion = table.Column<int>(type: "int", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
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

            migrationBuilder.CreateTable(
                name: "JuicioResultado",
                schema: "adso",
                columns: table => new
                {
                    id_juicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_resultado = table.Column<int>(type: "int", nullable: false),
                    id_aprendiz = table.Column<int>(type: "int", nullable: false),
                    juicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Por Evaluar"),
                    registrado_por = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    fecha_juicio = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuicioResultado", x => x.id_juicio);
                    table.CheckConstraint("CK_Juicio_juicio", "juicio IN ('Aprobado','No Aprobado','Por Evaluar')");
                    table.ForeignKey(
                        name: "FK_Juicio_Aprendiz",
                        column: x => x.id_aprendiz,
                        principalSchema: "adso",
                        principalTable: "AprendizPerfil",
                        principalColumn: "id_aprendiz",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Juicio_Resultado",
                        column: x => x.id_resultado,
                        principalSchema: "adso",
                        principalTable: "ResultadoAprendizaje",
                        principalColumn: "id_resultado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportacionError",
                schema: "adso",
                columns: table => new
                {
                    id_error = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_importacion = table.Column<int>(type: "int", nullable: false),
                    numero_fila = table.Column<int>(type: "int", nullable: false),
                    datos_fila = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    motivo_error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportacionError", x => x.id_error);
                    table.ForeignKey(
                        name: "FK_ImportErr_Import",
                        column: x => x.id_importacion,
                        principalSchema: "adso",
                        principalTable: "ImportacionArchivo",
                        principalColumn: "id_importacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AprendizPerfil_id_persona",
                schema: "adso",
                table: "AprendizPerfil",
                column: "id_persona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AprendizPerfil_id_usuario",
                schema: "adso",
                table: "AprendizPerfil",
                column: "id_usuario",
                unique: true,
                filter: "[id_usuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_id_usuario_subio",
                schema: "adso",
                table: "Archivo",
                column: "id_usuario_subio");

            migrationBuilder.CreateIndex(
                name: "IX_Archivo_Tipo_Fecha",
                schema: "adso",
                table: "Archivo",
                columns: new[] { "id_tipo_archivo", "fecha_subida" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Aprendiz",
                schema: "adso",
                table: "Asistencia",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "UQ_Asistencia",
                schema: "adso",
                table: "Asistencia",
                columns: new[] { "id_sesion", "id_aprendiz" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competencia_codigo",
                schema: "adso",
                table: "Competencia",
                column: "codigo",
                unique: true);

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
                name: "IX_DesarrolloCurricular_Competencia",
                schema: "adso",
                table: "DesarrolloCurricular",
                column: "id_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_DesarrolloCurricular_id_documento",
                schema: "adso",
                table: "DesarrolloCurricular",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_DesarrolloCurricular_id_instructor",
                schema: "adso",
                table: "DesarrolloCurricular",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_Ficha_numero_ficha",
                schema: "adso",
                table: "Ficha",
                column: "numero_ficha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FichaAprendiz_id_aprendiz",
                schema: "adso",
                table: "FichaAprendiz",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_FC_Instructor_Fecha",
                schema: "adso",
                table: "FichaCompetencia",
                columns: new[] { "id_instructor", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_FichaCompetencia_id_competencia",
                schema: "adso",
                table: "FichaCompetencia",
                column: "id_competencia");

            migrationBuilder.CreateIndex(
                name: "UX_FC_Ficha_Competencia",
                schema: "adso",
                table: "FichaCompetencia",
                columns: new[] { "id_ficha", "id_competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuiaAprendizaje_FC",
                schema: "adso",
                table: "GuiaAprendizaje",
                column: "id_ficha_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_GuiaAprendizaje_id_documento",
                schema: "adso",
                table: "GuiaAprendizaje",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_GuiaAprendizaje_id_instructor",
                schema: "adso",
                table: "GuiaAprendizaje",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_Import_Tipo_Fecha",
                schema: "adso",
                table: "ImportacionArchivo",
                columns: new[] { "tipo_entidad", "fecha_subida" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Import_Usuario",
                schema: "adso",
                table: "ImportacionArchivo",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_ImportacionArchivo_id_documento",
                schema: "adso",
                table: "ImportacionArchivo",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_ImportacionArchivo_id_sesion_destino",
                schema: "adso",
                table: "ImportacionArchivo",
                column: "id_sesion_destino");

            migrationBuilder.CreateIndex(
                name: "IX_ImportErr_Importacion",
                schema: "adso",
                table: "ImportacionError",
                column: "id_importacion");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPerfil_id_usuario",
                schema: "adso",
                table: "InstructorPerfil",
                column: "id_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentoEvaluacion_Competencia",
                schema: "adso",
                table: "InstrumentoEvaluacion",
                column: "id_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentoEvaluacion_id_documento",
                schema: "adso",
                table: "InstrumentoEvaluacion",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentoEvaluacion_id_instructor",
                schema: "adso",
                table: "InstrumentoEvaluacion",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_JuicioResultado_Aprendiz",
                schema: "adso",
                table: "JuicioResultado",
                column: "id_aprendiz");

            migrationBuilder.CreateIndex(
                name: "IX_JuicioResultado_Resultado",
                schema: "adso",
                table: "JuicioResultado",
                column: "id_resultado");

            migrationBuilder.CreateIndex(
                name: "UQ_Juicio",
                schema: "adso",
                table: "JuicioResultado",
                columns: new[] { "id_resultado", "id_aprendiz" },
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

            migrationBuilder.CreateIndex(
                name: "IX_PRT_Usuario",
                schema: "adso",
                table: "PasswordResetToken",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UX_PRT_TokenHash",
                schema: "adso",
                table: "PasswordResetToken",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Documento",
                schema: "adso",
                table: "Persona",
                column: "numero_documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_id_tipo_documento",
                schema: "adso",
                table: "Persona",
                column: "id_tipo_documento");

            migrationBuilder.CreateIndex(
                name: "IX_PlanConcertado_id_competencia",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_PlanConcertado_id_documento",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_PlanConcertado_id_instructor",
                schema: "adso",
                table: "PlanConcertado",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneacionPedagogica_FC",
                schema: "adso",
                table: "PlaneacionPedagogica",
                column: "id_ficha_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneacionPedagogica_id_documento",
                schema: "adso",
                table: "PlaneacionPedagogica",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_PlaneacionPedagogica_id_instructor",
                schema: "adso",
                table: "PlaneacionPedagogica",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoFormativo_id_documento",
                schema: "adso",
                table: "ProyectoFormativo",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoFormativo_id_ficha",
                schema: "adso",
                table: "ProyectoFormativo",
                column: "id_ficha");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoFormativo_id_instructor",
                schema: "adso",
                table: "ProyectoFormativo",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoAprendizaje_Plan",
                schema: "adso",
                table: "ResultadoAprendizaje",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "UQ_Res_Plan_Codigo",
                schema: "adso",
                table: "ResultadoAprendizaje",
                columns: new[] { "id_plan", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rol_nombre",
                schema: "adso",
                table: "Rol",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sesion_id_ficha_competencia",
                schema: "adso",
                table: "Sesion",
                column: "id_ficha_competencia");

            migrationBuilder.CreateIndex(
                name: "IX_Sesion_id_instructor",
                schema: "adso",
                table: "Sesion",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_TipoArchivo_nombre",
                schema: "adso",
                table: "TipoArchivo",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoDocumento_nombre",
                schema: "adso",
                table: "TipoDocumento",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                schema: "adso",
                table: "Usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_persona",
                schema: "adso",
                table: "Usuario",
                column: "id_persona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_rol",
                schema: "adso",
                table: "Usuario",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencia",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Auditoria",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ConfiguracionAlerta",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "DesarrolloCurricular",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "FichaAprendiz",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "GuiaAprendizaje",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ImportacionError",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "InstrumentoEvaluacion",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "JuicioResultado",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Notificacion",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ObservacionAprendiz",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "PasswordResetToken",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "PlaneacionPedagogica",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ProyectoFormativo",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ImportacionArchivo",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "ResultadoAprendizaje",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "AprendizPerfil",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Sesion",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "PlanConcertado",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "FichaCompetencia",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Archivo",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Competencia",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Ficha",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "InstructorPerfil",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "TipoArchivo",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Persona",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "Rol",
                schema: "adso");

            migrationBuilder.DropTable(
                name: "TipoDocumento",
                schema: "adso");
        }
    }
}
