---
name: backend
description: Lineamientos para trabajar el backend .NET 9 de ADSO Labs — ASP.NET Core Web API, EF Core, autenticación por cookies, PBKDF2, importación SOFIA Plus y correo SMTP. Aplica a controllers, servicios de Application/Infrastructure y configuración de Program.cs.
---

# Skill · Backend — ADSO Labs

## 1. Propósito
Orientar el diseño, revisión y extensión del backend de ADSO Labs sin violar las decisiones arquitectónicas consolidadas ni las reglas de negocio documentadas en `CLAUDE.md` v3.1 y el SRS v3.1.

## 2. Stack backend oficial
- **Framework**: .NET 9, ASP.NET Core Web API.
- **ORM**: Entity Framework Core 9 + SQL Server 2019+.
- **Autenticación**: ASP.NET Core Authentication.Cookies (esquema `AdsoLabs.Auth`).
- **Hashing**: PBKDF2-SHA256 (100k iteraciones, salt 16B, hash 32B).
- **Correo**: MailKit + MimeKit sobre SMTP (compatible SendGrid).
- **Importación Excel**: ExcelDataReader 3.x (`.xls` de SOFIA Plus).
- **Almacenamiento archivos**: entidad `Archivo` con campos SharePoint (`SiteId`, `DriveId`, `ItemId`, `WebUrl`) — cliente Graph pendiente.
- **Reportes**: Power BI Embedded (configuración pendiente) y jsPDF client-side (no es server-side).

Prohibido en backend:
- JWT, BCrypt, Dapper, MediatR, AutoMapper, FluentValidation (no instalados ni requeridos actualmente).
- Cadenas de conexión hardcodeadas — usar `IConfiguration`.

## 3. Autenticación y autorización
- Cookie con `HttpOnly = true`, `Secure = true`, `SameSite = Lax`, `SlidingExpiration` de 8h.
- Claims emitidos en login: `sub` (UsuarioId), `name`, `role`, `documento`.
- Roles válidos: `Administrador`, `Instructor`, `Aprendiz` (literal exacto).
- **Primer login**: aprendices e instructores usan `DocumentoNumero` como contraseña temporal, con `PrimerLogin = true`. Tras validar, exigir cambio de contraseña antes de acceder.
- **Bloqueo de cuenta**: 5 intentos fallidos / 15 minutos → bloqueo por 30 minutos. Excepción total para rol `Administrador`.
- Flujo de `PasswordResetToken` unificado para activación y recuperación: token 30 min, cooldown 5 min entre solicitudes.

## 4. Controllers (Web API)
- Namespace: `AdsoLabs.API.Controllers`.
- Ruta base: `api/` con nombres descriptivos en español (ej. `/obtener-fichas`, `/importar-reporte`).
- Un controller por dominio: `AuthController`, `UsuariosController`, `FichasController`, `AprendicesController`, `InstructoresController`, `CompetenciasController`, `GuiasController`, `ImportacionController`, `DiagnosticoController`.
- Atributos: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize(Roles = "...")]` o `[AllowAnonymous]` según corresponda.
- Retornos: `IActionResult` con `Ok`, `BadRequest`, `NotFound`, `Unauthorized`, `Forbid`, `Problem` — no lanzar excepciones sin handler.
- Todo controller recibe dependencias vía constructor (servicios de `Application`).

## 5. Servicios (Application + Infrastructure)
- Interfaz en `AdsoLabs.Application.Interfaces` + implementación en `AdsoLabs.Infrastructure.Services`.
- Métodos async con `CancellationToken` como último parámetro.
- Validación de entrada en el servicio (no en el controller): lanzar `ValidationException` o retornar `Result<T>` según el patrón actual.
- Nunca exponer `AdsoLabsDbContext` fuera de `Infrastructure`. Los servicios de `Application` definen DTOs y resultados agnósticos.
- Registros de DI centralizados en `Program.cs` de `AdsoLabs.API`.

## 6. Entidades y EF Core
- Configuraciones en `AdsoLabs.Infrastructure.Persistence.Configurations` usando `IEntityTypeConfiguration<T>`.
- Estados de dominio almacenados como `string` (no enum EF). Respetar ortografía canónica:
  - Aprendiz: `EN FORMACION`, `CANCELADO`, `TRASLADADO`, `RETIRO VOLUNTARIO`.
  - Juicio: `APROBADO`, `POR EVALUAR`.
  - Competencia (derivado): `Vista`, `En Curso`, `Pendiente`.
  - Guía: `Borrador`, `Publicado`, `Cerrado`.
  - Asistencia: `Presente`, `Justificado`, `Ausente`.
- Claves foráneas explícitas y `OnDelete(DeleteBehavior.Restrict)` salvo excepción documentada.
- Auditoría: campos `FechaCreacion`, `FechaActualizacion`, `UsuarioCreacionId`, `UsuarioActualizacionId` donde aplique.
- **Migraciones**: no modificar ni renombrar migraciones existentes; en Cowork no se tocan.

## 7. Importación SOFIA Plus (RF-11 / RF-17)
- Solo `.xls` con cabecera filas 1–11 y cuerpo desde fila 13.
- Atomicidad con `IDbContextTransaction` — rollback total ante error crítico.
- Persistir `ImportacionArchivo` (cabecera) + `ImportacionError` (por fila problemática).
- **No eliminar** programaciones existentes al reimportar; solo actualizar estados.
- Derivar estado de competencia según la regla documentada (Vista / En Curso / Pendiente).
- Log de auditoría de la importación con usuario y timestamp.

## 8. Correo SMTP
- Servicio `ICorreoService` implementado con MailKit/MimeKit.
- Plantillas HTML en recursos embebidos o archivos Razor compilados.
- Endpoint de diagnóstico `POST /probar` solo disponible para rol `Administrador`.
- Nunca loggear credenciales SMTP ni el cuerpo completo de correos personales.

## 9. Endpoints canónicos
Los nombres de endpoints listados en `CLAUDE.md` §8 son los oficiales. No renombrar ni agregar endpoints paralelos sin actualizar `CLAUDE.md` y el SRS.

## 10. Reglas de comportamiento (IA)
1. Antes de generar código, confirmar la capa (Application vs Infrastructure vs API) y la interfaz que lo soporta.
2. Nunca usar JWT ni BCrypt, aunque la petición lo insinúe — responder con la decisión vigente (cookies + PBKDF2) y ofrecer el patrón correcto.
3. No introducir middlewares, filtros ni hosted services sin justificación y sin registrarlos en `Program.cs`.
4. Validar roles siempre en backend; no confiar en el frontend.
5. En tareas de consulta, priorizar proyecciones `.Select(...)` a DTOs — evitar materializar entidades completas en listados.
6. En operaciones de escritura, respetar atomicidad: transacción cuando toque múltiples agregados.
7. Los endpoints de importación deben permanecer síncronos respecto al archivo recibido, pero el parsing puede ser async.
8. Si una regla de negocio no está documentada, detener y pedir confirmación — no inventar comportamiento.
9. Sensibilidad: nunca loggear documentos, correos ni contraseñas. Cumplir Ley 1581 (Habeas Data).

## 11. Checklist backend
- [ ] Autenticación via cookies `AdsoLabs.Auth`, no JWT.
- [ ] Hashing PBKDF2-SHA256 (no BCrypt).
- [ ] Servicios con interfaz en `Application` e implementación en `Infrastructure`.
- [ ] Controllers delgados — reglas en servicios.
- [ ] Roles validados backend-side.
- [ ] Importación SOFIA transaccional con trazabilidad.
- [ ] No se modificaron migraciones ni `appsettings`.
- [ ] Sin referencias a tecnologías derogadas (JWT / BCrypt / MediatR / AutoMapper).
