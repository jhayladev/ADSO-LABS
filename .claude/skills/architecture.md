---
name: architecture
description: Reglas de Clean Architecture para ADSO Labs. Define las dependencias permitidas entre capas, las responsabilidades de cada proyecto y los patrones obligatorios para mantener la integridad arquitectónica.
---

# Skill · Architecture — ADSO Labs

## 1. Propósito
Evitar violaciones arquitectónicas durante generación o revisión de código. Esta skill se activa ante cualquier tarea que implique añadir, mover o proponer código entre capas (`Core`, `Application`, `Infrastructure`, `API`, `Web`).

## 2. Estructura de la solución
```
AdsoLabs.sln
├── AdsoLabs.Core            → Dominio puro
├── AdsoLabs.Application     → Casos de uso + contratos
├── AdsoLabs.Infrastructure  → EF Core, SMTP, hashing, identidad, migraciones
├── AdsoLabs.API             → ASP.NET Core Web API
└── AdsoLabs.Web             → Blazor Server
```
> Los proyectos residen en la raíz del repositorio (no bajo `src/`). No existe carpeta `tests/` con una suite activa.

## 3. Grafo de dependencias permitidas
```
Web  ──► API ──► Application ──► Core
                 ▲
Infrastructure ──┘   (Infrastructure implementa Application, depende de Core)
```

Reglas:
- `Core` **no depende de nadie**.
- `Application` depende **solo** de `Core`.
- `Infrastructure` depende de `Application` y `Core`. Puede referenciar EF Core, MailKit, MimeKit, ExcelDataReader, Microsoft.AspNetCore.Authentication.Cookies (para `PasswordHasher`, `ICurrentUser`, etc.).
- `API` depende de `Application` e `Infrastructure` (para DI de implementaciones concretas).
- `Web` (Blazor) consume **únicamente** los endpoints de `API` vía `HttpClient`. No debe referenciar `Application`, `Infrastructure` ni `Core` directamente.

Violaciones prohibidas:
- `Core → Application`, `Core → Infrastructure`, `Core → API`.
- `Application → Infrastructure`, `Application → API`.
- `Web → Infrastructure`, `Web → Core`, `Web → Application`.

## 4. Responsabilidades por capa

### Core
- Entidades: `Usuario`, `Rol`, `Ficha`, `Aprendiz`, `FichaAprendiz`, `Instructor`, `Ambiente`, `Competencia`, `CompetenciaResultado`, `FichaCompetenciaResultado`, `GuiaAprendizaje`, `InstrumentoEvaluacion`, `PlanConcertado`, `JuicioResultado`, `Sesion`, `Asistencia`, `Informe`, `PasswordResetToken`, `ImportacionArchivo`, `ImportacionError`, `TipoArchivo`, `Archivo`.
- Enums (`EstadoAprendiz`, `EstadoFicha`, `EstadoGuia`, etc.).
- Excepciones de dominio.
- **Prohibido**: anotaciones EF Core (`[Column]`, `[Table]`), referencias a `DbContext`, lógica de infraestructura.

### Application
- Interfaces de servicios (`IUsuarioService`, `IFichaService`, `IImportacionService`, `ICorreoService`, `IPasswordHasher`, `ICurrentUserService`, etc.).
- DTOs (Request/Response) y validaciones.
- Casos de uso / servicios que orquestan reglas de negocio.
- **Prohibido**: invocar `DbContext` directamente, escribir SQL, enviar correos, leer archivos. Todo eso se abstrae vía interfaz.

### Infrastructure
- `AdsoLabsDbContext` (EF Core) y configuraciones `IEntityTypeConfiguration`.
- Implementaciones de interfaces (`UsuarioService`, `PbkdfPasswordHasher`, `SmtpCorreoService`, `SofiaPlusImportService`, `CurrentUserService`).
- Migraciones (`AdsoLabs.Infrastructure/Migrations/**`) — **no tocar** en modo Cowork.
- Integraciones externas (MailKit, ExcelDataReader, Microsoft Graph cuando se habilite).

### API
- Controllers REST agrupados por dominio (`UsuariosController`, `FichasController`, `ImportacionController`, `AuthController`, `DiagnosticoController`).
- Configuración de autenticación por cookies (`AdsoLabs.Auth`, HttpOnly, Secure, SlidingExpiration 8h).
- Middleware de autorización basada en claims y roles (`Administrador`, `Instructor`, `Aprendiz`).
- Composition root / inyección de dependencias en `Program.cs`.
- Seeder y auto-migración al arranque (RF-27).

### Web (Blazor Server)
- Componentes `.razor` organizados por módulos (`Usuarios`, `Fichas`, `Competencias`, `Importacion`, `Guias`, `Informes`, `Dashboard`).
- `HttpClient` tipado que consume la API.
- Manejo de sesión vía cookies compartidas con la API.
- Renderizado server-side con SignalR.

## 5. Patrones obligatorios
- **DTO en frontera**: las controllers y los servicios `Application` intercambian DTOs, nunca entidades `Core` directamente.
- **Inyección de dependencias** en `Program.cs` de `API` y `Web`. Nada de `new` de servicios concretos en consumidores.
- **Async/await** en toda la ruta I/O (EF Core, SMTP, HttpClient).
- **ICurrentUserService** para obtener `UsuarioId`, `Roles`, `DocumentoNumero` desde claims.
- **CancellationToken** propagado en métodos async que tocan I/O.
- **Registro de auditoría**: toda entidad relevante expone `FechaCreacion`, `FechaActualizacion`, `UsuarioCreacionId`, `UsuarioActualizacionId` (RN-AUDIT-01 a RN-AUDIT-04).

## 6. Reglas de comportamiento (IA)
1. Antes de proponer un archivo nuevo, identificar a qué capa pertenece y validar el grafo de dependencias.
2. Si un caso de uso requiere acceso a datos, debe pasar por una interfaz en `Application` implementada en `Infrastructure`.
3. Nunca sugerir referenciar `AdsoLabsDbContext` desde `Web` o `Application`.
4. Si una funcionalidad nueva cruza capas, describir primero el diseño (interfaz + implementación + punto de DI) antes de escribir código.
5. No reemplazar cookies por JWT ni PBKDF2 por BCrypt; ambos fueron decisiones arquitectónicas firmes.
6. No introducir mediator/CQRS, repositorios genéricos ni patrones adicionales sin que el usuario los haya pedido explícitamente (el proyecto usa servicios por caso de uso).
7. En caso de detectar una violación existente, reportarla como hallazgo — **no** corregirla automáticamente en modo Cowork.

## 8. Checklist arquitectónico
- [ ] La capa propuesta respeta el grafo de dependencias.
- [ ] No se filtra EF Core hacia `Core`/`Application`.
- [ ] Los DTOs no exponen entidades del dominio.
- [ ] Se usa inyección de dependencias y async/await.
- [ ] Auditoría y `ICurrentUserService` contemplados.
- [ ] No se introducen dependencias nuevas sin justificación.
