# ADSO Labs — Guía del Proyecto

> **Versión:** 4.2
> **Última actualización:** Julio 2026
> **Estado:** Documento vivo — debe reflejar fielmente el estado real del sistema.
> **Alcance:** Referencia de arquitectura, stack y reglas de negocio para cualquier persona o agente de IA (Claude Code, Codex, Cursor, Windsurf, Gemini CLI, etc.) que trabaje en este repositorio. Para la guía de onboarding humano (cómo clonar y correr el proyecto), ver [`README.md`](README.md).

---

## 1. Descripción del Proyecto

ADSO Labs es una plataforma web de gestión académica para el programa **Análisis y Desarrollo de Software Orientado (ADSO)** del SENA. Centraliza la gestión de fichas de formación, aprendices, instructores, competencias, asistencia, monitorías, patrocinios, juicios evaluativos, repositorio documental y predicción de compatibilidad (IA).

- **Metodología:** Scrum / Ágil
- **Clasificación:** Confidencial — No compartir datos de aprendices o instructores en logs públicos, commits ni ejemplos.
- **Marco legal:** Ley 1581 de Colombia (Habeas Data).

---

## 2. Arquitectura de la Solución

La solución `AdsoLabs.sln` sigue una arquitectura en capas limpia (Clean Architecture):

```
AdsoLabs.sln
├── AdsoLabs.API          → API REST (.NET 9) — controladores, endpoints, composition root
├── AdsoLabs.Application  → Casos de uso (AppServices), DTOs, comandos, interfaces de servicios/queries
├── AdsoLabs.Core         → Entidades base de dominio y enums de resultado
├── AdsoLabs.Infrastructure → EF Core, SQL Server, Email (MailKit), Excel (ExcelDataReader), Security (Hash/Token)
└── AdsoLabs.Web          → Frontend Blazor Server — componentes, páginas, handlers Razor, servicios de API
```

### Regla fundamental de dependencias

```
Web → API → Application → Core ← Infrastructure
```

- `Core` no depende de ninguna otra capa. Solo contiene entidades base de identidad (`Persona`, `Usuario`, `Rol`, `TipoDocumento`, `Ficha`, `Competencia`, `FichaCompetencia`, `AprendizPerfil`, `InstructorPerfil`, `PasswordResetToken`) y enums de resultado.
- **Todas las demás entidades** (Asistencia, Patrocinio, Monitoría, Notificación, Auditoría, repositorio documental, etc.) viven en `AdsoLabs.Infrastructure/Models`, no en `Core` — decisión de diseño consciente, ver §13.
- `Infrastructure` implementa interfaces definidas en `Application` (servicios y repositorios).
- `Web` y `API` consumen exclusivamente `Application`; nunca `Infrastructure` ni entidades de `Core` directamente. `Web` tampoco llama a servicios externos directamente: consume `AdsoLabs.API` vía `HttpClient` (un `*ApiService.cs` por módulo).

Para el detalle completo de módulos, `AppServices` y endpoints, ver [`README.md`](README.md) — este documento se enfoca en reglas de negocio y decisiones que no cambian tan seguido.

---

## 3. Stack Tecnológico Real

| Capa | Tecnología | Nota |
|------|-----------|------|
| Frontend | Blazor Server (.NET 9) + Razor Pages (handlers) | Interactive Server render mode |
| Backend | ASP.NET Core Web API (.NET 9) | |
| ORM | Entity Framework Core + SQL Server 2019+ | Migraciones automáticas al arranque (28 migraciones a la fecha) |
| Autenticación (navegador) | **ASP.NET Core Authentication.Cookies** | Esquema `AdsoLabs.Auth`, HttpOnly + Secure, 8h sliding expiration, claims-based. Configurada solo en `AdsoLabs.Web`. |
| Autenticación (Web↔API) | **JWT interno de 2 minutos** | `AdsoLabs.Web` mintea un token corto por cada llamada saliente a partir de la cookie ya validada (`InternalTokenHandler.cs`); `AdsoLabs.API` lo valida vía `AddJwtBearer` con clave simétrica compartida (`InternalAuth:Key`, local, gitignored). El navegador nunca ve este token — no contradice la decisión de "cookies, no JWT" de cara al cliente. |
| Hashing | **PBKDF2-SHA256 (100k iteraciones, salt 16 bytes, hash 32 bytes)** | Implementado en `HashService` |
| Email | MailKit / MimeKit | SMTP, compatible SendGrid |
| Importación Excel | ExcelDataReader 3.x | `.xls` desde SOFIA Plus; plantillas propias `.xlsx` para fichas/aprendices |
| Tiempo real | SignalR (`NotificacionHub`) | Grupo por usuario (`user-{idUsuario}`); no pasa por el mismo pipeline de autorización que los controllers — pendiente. |
| IA | **Anthropic Claude Haiku** (`claude-haiku-4-5-20251001`) | Migrado desde Groq/LLaMA 3.3. Predicción de compatibilidad/aprendizaje para asignación de monitores/instructores (`PrediccionIAService`), respetando la fase formativa de la ficha/competencia (`FaseFormativaHelper`) |
| Gráficos / Dashboard | Chart.js vía JSInterop | Implementado — sin Power BI, todo autohospedado en Blazor |
| Informes PDF | jsPDF del lado cliente (sobre `Informe.razor`) | No hay generador server-side de PDF |
| Reportes Excel | Server-side (`InformeExcelGenerator`) | Se consume por `HttpClient` autenticado + JS interop, no por `<a href>` directo a la API |
| UI | Bootstrap 5 | |
| Almacenamiento archivos | `LocalArchivoStorageService` (disco local) | Decisión firme: **no se integra SharePoint** (ver §4). La entidad `Archivo` conserva campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` sin uso, legado del diseño original. |

> **Nota histórica:** versiones anteriores de este documento mencionaban JWT y BCrypt como stack de autenticación **de cara al navegador**. Eso sigue sin ser cierto — la cookie sigue siendo el mecanismo browser-facing. El JWT interno agregado en julio 2026 es exclusivamente servidor-a-servidor (Web↔API) y no cambia esa decisión.

---

## 4. Módulos del Sistema

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| Autenticación | ✅ Implementado | Login por cookie, bloqueo por intentos, primer login con documento |
| Autorización de la API | ✅ Implementado | `[Authorize(Roles=...)]` por controller + JWT interno Web↔API (ver §3 y §7.1) |
| Activación y Recuperación de Cuenta | ✅ Implementado | Tokens con expiración 30 min y cooldown 5 min |
| Dashboard | ✅ Implementado | `Home.razor` con resumen, donut de competencias, barras de progreso y alertas — todo vía `/api/mi-dashboard` |
| Gestión de Fichas | ✅ Implementado | Listado, detalle, aprendices, competencias |
| Gestión de Competencias | ✅ Implementado | Catálogo, filtros, estados |
| Gestión de Aprendices | ✅ Implementado | Importación SOFIA Plus, métricas, cambio de estado |
| Gestión de Instructores | ✅ Implementado | CRUD, filtro por competencia |
| Asistencia | ⚠️ Parcial | Modelo `Sesion`+`Asistencia` implementado; la lógica real de "En Riesgo" (3 inasistencias consecutivas o 20% de horas por competencia) ya está implementada vía `AsistenciaRiesgoCalculator`, reutilizada de forma consistente en Aprendiz/Informe/Reportes (ver §7.6). **Pendiente por construir:** inmutabilidad de registros confirmados |
| Patrocinio | ✅ Implementado | Etapas Lectiva/Productiva + empresa |
| Monitorías | ✅ Implementado | Asignación de monitores, sesiones, inscripción, asistencia e informes — mergeado a `main` |
| Predicción IA | ✅ Implementado | Compatibilidad y predicción de aprendizaje vía Anthropic Claude Haiku, respetando la fase formativa de la ficha/competencia, para asignación de monitores/instructores |
| Fase formativa de competencias | ✅ Implementado | Cada competencia del catálogo tiene una fase (Inducción/Análisis/Planeación/Ejecución/Evaluación/Transversal); visible en Fichas > Competencias, Asignación de Competencias y la pestaña "Ruta Formativa" de Fichas |
| Notificaciones | ✅ Implementado | Bandeja en tiempo real vía SignalR (`NotificacionHub`) |
| Repositorio Documental | ✅ Implementado | Entidades múltiples (Guía, Instrumento, Planeación, Proyecto, Desarrollo Curricular, Plan Concertado) con UI completa, almacenamiento en disco local |
| Perfil del Aprendiz | ✅ Implementado | `Profile.razor`: datos propios, acudiente, observaciones, **cronograma visual** (calendario de sesiones e info de patrocinio, tab "Cronograma") y detalle de resultados por competencia |
| Integración SharePoint | 🚫 Descartado | Se decidió **no** implementarla — el repositorio documental usa almacenamiento local (`LocalArchivoStorageService`) de forma permanente, no como paso intermedio. Los campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` en la entidad `Archivo` quedan sin uso (legado del diseño original). |
| Evidencias de Aprendizaje | 🚫 Descartado | Fuera de alcance — decisión de producto, no se va a implementar. |
| Informe Académico PDF | ✅ Implementado | jsPDF con datos reales vía `InformeAprendizApiService` |
| Reportes | ✅ Implementado | `Reportes.razor` con 8 tipos de reporte en pantalla + exportación a Excel server-side |
| Auditoría transversal | ✅ Implementado | `AuditoriaInterceptor` (EF Core `SaveChangesInterceptor`) alimenta la tabla `Auditoria` para `AprendizPerfil`, `InstructorPerfil`, `FichaAprendiz` (cambios de estado), `Archivo` (carga/eliminación) y cambios de `Usuario.PasswordHash` |
| Historial de Estado del Aprendiz | ✅ Implementado | `HistorialEstadoAprendiz` registra cada cambio de estado (`EstadoAnterior`, `EstadoNuevo`, `IdUsuarioCambio`, `FechaCambio`) |
| Política de contraseñas y consentimiento informado | ✅ Implementado | `PasswordPolicyValidator` (mínimo 8 caracteres, mayúscula, número, carácter especial) validado en `AuthController`/`PasswordController`; checkbox de consentimiento obligatorio en `PasswordChange.razor` con aviso de tratamiento de datos (borrador, pendiente de revisión legal) — ver §7.1 y §11 |

Leyenda: ✅ implementado · ⚠️ parcial · ❌ no implementado (pendiente, sigue en el roadmap) · 🚫 descartado (decisión de producto, no se va a construir).

---

## 5. Roles de Usuario

| Rol | Permisos |
|-----|----------|
| **Administrador** | Control total: importar fichas, gestionar usuarios, ver todos los reportes. Nunca se bloquea por intentos fallidos. |
| **Instructor** | Gestión pedagógica de sus fichas: competencias, asistencia, aprendices asignados, guías de sus competencias. |
| **Aprendiz** | Perfil propio (lectura + edición de contacto), sus competencias, sus notificaciones. Puede ser **monitor** de una competencia (no es un rol distinto, es un estado sobre `AprendizPerfil`/`MonitorPerfil`). |

**Control de acceso:** RBAC basado en claims. Los roles válidos son los literales exactos: `Administrador`, `Instructor`, `Aprendiz`. Se valida **en backend** vía `[Authorize(Roles = "...")]` en cada controller de `AdsoLabs.API` (no solo en la UI de Blazor) — ver §7.1 y la tabla de endpoints en `README.md`.

---

## 6. Convenciones de Código

### Estructura de `AdsoLabs.Web`

```
AdsoLabs.Web/
├── Components/
│   ├── Layout/       → MainLayout, LoginLayout, NavMenu
│   └── Pages/        → Un archivo .razor por módulo (Aprendiz.razor, Fichas.razor, etc.)
├── Pages/            → Handlers Razor Pages (LoginHandler, LogoutHandler, ActivacionHandler, RecuperacionHandler)
├── Services/
│   ├── SesionService.cs        → Estado de sesión del usuario vía Claims (IdUsuario, Rol, PrimerLogin)
│   ├── InternalTokenHandler.cs → Mintea el JWT interno para cada llamada a la API (ver §3)
│   └── Api/                    → Un *ApiService.cs por módulo (AuthApiService, AprendizApiService, etc.)
└── wwwroot/                    → Estáticos: app.css, images/, lib/bootstrap/
```

**Patrón para nuevas páginas:**
1. Crear `NombreModulo.razor` en `Components/Pages/`.
2. Crear `NombreModuloApiService.cs` en `Services/Api/`, registrarlo en `Program.cs` con `.AddHttpMessageHandler<InternalTokenHandler>()`.
3. Agregar CSS aislado en `NombreModulo.razor.css` si se requiere.

### Estructura de `AdsoLabs.API`

- Un controlador por módulo en `Controllers/`.
- Rutas bajo prefijo `api/`, con nombres descriptivos en español (`/obtener-aprendices`, `/importar-reporte`, etc.).
- Respuestas con `IActionResult` y códigos HTTP semánticos.
- **Todo controller exige autenticación por defecto** (filtro global en `Program.cs`); `[AllowAnonymous]` solo en login/activación/recuperación. `[Authorize(Roles = "...")]` por controller espejando la página Blazor correspondiente.
- Los endpoints "SELF" (dashboard propio, perfil propio, mis descargas, etc.) derivan el usuario del token con `ControllerBaseExtensions.ObtenerIdUsuarioAutenticado()` — **no** confían en un `idUsuario` de query string.

### Estructura de `AdsoLabs.Application`

- `AppService/<Modulo>/` — un `AppService` por módulo, orquesta el caso de uso completo (ver `README.md` para la explicación detallada de qué es un AppService).
- `DTOs/<Modulo>/` agrupados por módulo; `DTOs/<Modulo>/*Command.cs` para comandos de escritura.
- `Interfaces/Services/` (lógica de escritura) e `Interfaces/Queries/` (lecturas/proyecciones) — implementados en `Infrastructure`.
- **Sin FluentValidation** (no existe carpeta `Validators/` con reglas activas).

### Estructura de `AdsoLabs.Infrastructure`

- `Data/` con `AdsoDbContext` y configuraciones EF Core (`Data/Configuration/`, `IEntityTypeConfiguration<T>` por entidad, assembly-scan).
- `Migrations/` — convención `YYYYMMDDHHMMSS_Descripcion` (NO modificar migraciones existentes manualmente).
- `Repositories/` implementa interfaces del dominio.
- `Services/` con `EmailServices` (MailKit), `ImportadorReporteJuicios`/`ImportadorFichas`/`ImportadorAprendices` (ExcelDataReader), `ActivacionCuentaService`, `AuthService`, `RecuperacionPasswordService`, `MonitoriaService`, `PrediccionIAService`, `NotificacionService`, etc.
- `Security/` con `HashService` (PBKDF2) y lógica de tokens.

### Nomenclatura

- **Clases/Métodos:** PascalCase
- **Variables/Parámetros:** camelCase
- **Constantes:** SCREAMING_SNAKE_CASE
- **Archivos Razor:** PascalCase (ej. `Fichas.razor`)
- **Tablas SQL:** PascalCase singular (ej. `Aprendiz`, `Ficha`, `Competencia`)
- **Columnas SQL:** PascalCase
- **Estados de dominio:** Se almacenan como `string` (no EF-enum). Mantener la ortografía exacta documentada (ej. `EN FORMACION`, `Presente`, `TRASLADADO`).

---

## 7. Reglas de Negocio Críticas

> Estas reglas deben respetarse siempre. Antes de tocar cualquier lógica de estado, importación o autorización, revisar este bloque.

### 7.1 Autenticación y Control de Acceso

- **Autenticación de cara al navegador:** cookie ASP.NET Core (esquema `AdsoLabs.Auth`), configurada solo en `AdsoLabs.Web`. NO usar JWT expuesto al navegador.
- **Autenticación Web↔API:** JWT interno de 2 minutos (ver §3). Si vas a agregar un endpoint nuevo a la API, recuerda que por defecto **exige estar autenticado** — usa `[AllowAnonymous]` solo si de verdad debe ser público, y `[Authorize(Roles = "...")]` para restringir por rol.
- **Hashing:** `IHashService` implementa PBKDF2-SHA256 con salt único por usuario.
- **Primer Login:**
  - Tanto **instructores** como **aprendices** (incluye el usuario Administrador sembrado) usan su **número de documento** como contraseña temporal.
  - El sistema detecta `PrimerLogin = true` y compara contra el documento en lugar del hash — esta comparación vive en `AuthService.ValidarCredencialesAsync`, sin importar el rol.
  - Tras validación exitosa, el usuario **debe cambiar** su contraseña antes de acceder al sistema.
  - Una vez cambiada, `PrimerLogin` pasa a `false` y futuras autenticaciones usan el hash PBKDF2.
- **Mensajes de error seguros:** Login no distingue entre usuario inexistente y contraseña incorrecta (siempre "Usuario o contraseña incorrectos").
- **Bloqueo de cuenta:** 5 intentos fallidos en 15 minutos → bloqueo por 30 minutos.
- **Excepción:** Usuarios con rol **Administrador** **NO se bloquean** bajo ninguna circunstancia.
- **Alcance del bloqueo:** Instructores y aprendices **SÍ** se bloquean.
- **Desbloqueo automático:** si pasaron 15 minutos desde el bloqueo, la cuenta se desbloquea al siguiente intento.
- **Estado `Activo = false`** → no puede iniciar sesión bajo ninguna circunstancia.
- **Sincronización con estado de aprendiz:** cambio a `CANCELADO` o `RETIRO VOLUNTARIO` marca `Usuario.Activo = false`.
- **Auditoría:** cada login exitoso actualiza `Usuario.UltimoAcceso`. Los intentos fallidos incrementan `IntentosFallidos`.

### 7.2 Activación de Cuenta (Aprendices e Instructores)

- **Prerrequisito:** el usuario ya debe existir en `Persona` + `AprendizPerfil`/`InstructorPerfil` sin `IdUsuario` asignado.
- **Flujo:** documento + correo → valida existencia → crea `Usuario` con `Activo=false, PrimerLogin=true` → genera token GUID hash SHA256 → envía correo.
- **Token:** expira a los **30 minutos** de su creación. Solo uno activo a la vez (los anteriores se invalidan).
- **Cooldown:** 5 minutos entre solicitudes de token. `ResultadoEnvioToken.EnCooldown` controla la respuesta.
- **Tabla única:** `PasswordResetToken` se usa tanto para activación como para recuperación.
- **Recuperación:** no cambia `Activo` ni `PrimerLogin`, solo actualiza `PasswordHash`/`PasswordSalt`.
- **Complejidad de contraseña:** validada por `PasswordPolicyValidator` (mínimo 8 caracteres, mayúscula, número, carácter especial) — ver §7.10.
- **Consentimiento informado (Ley 1581):** obligatorio en `PasswordChange.razor` (primer login) — checkbox validado también en servidor (`AuthController`); persiste `Usuario.ConsentimientoFecha`. El aviso de tratamiento de datos (`wwwroot/docs/aviso-tratamiento-datos.html`) es un **borrador pendiente de revisión legal**, no usar en producción sin validación del equipo legal.

### 7.3 Fichas y Competencias

- El progreso de una ficha = (horas de competencias completadas / total horas ficha) × 100.
- Estado de competencia en una ficha: **Pendiente** (sin programar), **Programada** (con instructor + fechas + horas), **Vista** (todos los resultados APROBADO con aprendices EN FORMACION).
- La suma de horas de los resultados de aprendizaje **debe** ser exactamente igual al total de horas de la competencia. No guardar si no se cumple.
- Una competencia **Clausurada** es visible en el catálogo pero **no asignable** a nuevas fichas.

### 7.4 Programación de Competencias

- La programación se realiza **por resultado de aprendizaje** individual (entidad `FichaCompetenciaResultado`), no por competencia completa.
- Un resultado solo puede programarse **una vez** por ficha-competencia.
- Cada programación requiere: `IdInstructor` no nulo, `Horas > 0`, `HoraInicio < HoraFin`.
- **Estado del registro:** `Pendiente`, `Programado`, `Completado`.
- **Fechas derivadas:** `FechaInicio` = fecha del primer resultado programado; `FechaFin` = fecha del último.
- **Restricciones de horario:** no se permiten conflictos del mismo instructor ni solapamientos en la misma ficha-competencia.
- El instructor debe existir y estar activo. La ficha no debe estar cerrada ni cancelada.
- La acción de configurar la ventana de programación (`configurar-competencia-ficha`) es **solo Administrador**; programar/eliminar un resultado individual es Administrador o Instructor.

### 7.5 Importación del Reporte de Juicios Evaluativos SOFIA Plus (RF-11)

> Componente: `ImportadorReporteJuicios` (`AdsoLabs.Infrastructure.Services`). Es la funcionalidad más importante y más delicada del sistema — ver la explicación completa (arquitectura de dos fases, reglas fila por fila, trazabilidad) en [`README.md` § Infrastructure](README.md#infrastructure--persistencia-y-servicios-técnicos). Este bloque resume solo lo que un agente **debe saber antes de tocar código relacionado**.

- Solo se acepta el archivo **Reporte de Juicios Evaluativos** en formato `.xls` exportado desde SOFIA Plus.
- Estados válidos del aprendiz: `EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO`, `TRASLADADO`. Normalización tolerante (`TRASLAD*` ⇒ `TRASLADADO`, etc.).
- Juicios válidos: `APROBADO` y `POR EVALUAR`. `POR EVALUAR` no genera `JuicioResultado`.
- **Etapa Práctica/Productiva** (`CodigoCompetencia == "590803"` o nombre con "ETAPA PRACTICA"/"ETAPA PRODUCTIVA") se omite del flujo académico completo.
- **No crea fichas ni aprendices** — si la ficha no existe se rechaza el archivo entero; si el aprendiz no existe, se rechaza esa fila y la importación continúa.
- **Atomicidad por fila, no global.** Idempotente (todo upsert). Nunca degrada el estado `Vista` de una `FichaCompetencia` al reimportar.
- **Trazabilidad obligatoria** vía `ImportacionArchivo` + `ImportacionError`.
- **No modificar sin petición expresa del usuario** — ver §10.

### 7.5.bis Importación masiva de Aprendices (RF-28)

> Componente: `ImportadorAprendices` (`AdsoLabs.Infrastructure.Services`).

- Detección automática de formato por extensión: `.xlsx` ⇒ plantilla oficial; `.xls` ⇒ Reporte SOFIA Plus (reutiliza `NormalizarReporteSofia`, dedup por documento).
- La ficha destino **siempre** viene del parámetro `numeroFicha` de la ruta; el número de ficha del Excel se ignora.
- Upsert de `Persona` + `AprendizPerfil`, vincula a la ficha aplicando traslado automático.
- **No** crea `Usuario` ni registra trazabilidad en `ImportacionArchivo`. Idempotente.

### 7.5.ter Importación masiva de Fichas (RF-29)

> Componente: `ImportadorFichas` (`AdsoLabs.Infrastructure.Services`).

- Solo `.xlsx` con plantilla oficial. El `Nombre` se asigna automáticamente a `"ANALISIS Y DESARROLLO DE SOFTWARE"`.
- Upsert por `NumeroFicha`: nuevas se crean `Activa`; existentes solo actualizan `Jornada`/`Modalidad`/fechas.
- **Auto-vincula** todas las competencias no `Clausurada` del catálogo a la ficha nueva en estado `Pendiente`.
- **No** crea aprendices ni `Usuario`, no registra trazabilidad en `ImportacionArchivo`.

### 7.6 Asistencia

- Solo aparecen aprendices con estado `EN FORMACION`.
- La unidad base es la entidad `Sesion` (Fecha, HoraInicio, HoraFin) asociada a `FichaCompetenciaResultado`.
- Estados de asistencia: `Presente`, `Justificado`, `Ausente`.
- Umbral de "En Riesgo": 3 inasistencias injustificadas consecutivas **o** 20% del total de horas de la competencia, evaluado por competencia (agrupando por `Sesion.IdFichaCompetencia`). **Implementado** en `AsistenciaRiesgoCalculator` (`Infrastructure/Queries/`), única fuente de verdad reutilizada por `AprendizQueryService` y `ReportesQueryService` para que ambas pantallas coincidan.
- Los registros de asistencia deben ser **inmutables** una vez confirmados. **Estado actual: no está garantizado por el código** — pendiente de implementar (solo admin debería corregir, con auditoría).

### 7.7 Patrocinio

- Un aprendiz patrocinado solo puede tener una etapa activa: **Lectiva** o **Productiva**.
- **Etapa Lectiva:** horario de patrocinio contrario (jornada diferente) a la jornada de formación.
- **Etapa Productiva:** duración fija de 6 meses calendario.
- El módulo es **solo lectura para instructores**; el administrador puede modificar.
- La entidad `Patrocinio` almacena `NombreEmpresa` y `ContactoEmpresa`.
- Submódulo `AsistenciasPatrocinado.razor` para seguimiento específico de asistencia de aprendices patrocinados.

### 7.8 Dashboard

- ✅ Implementado (`Home.razor` + `DashboardController` + `/api/mi-dashboard`). La información corresponde a las fichas asignadas del instructor (o todas, si es Administrador) — filtrado ya resuelto vía `ObtenerIdUsuarioAutenticado()`, no por parámetro del cliente.
- Construido **completamente en Blazor Server** con Chart.js vía JSInterop, sin Power BI.
- **Pendiente de definición de negocio (no bloqueante, pero sin resolver):**
  - Umbrales exactos de **Crítico / Riesgo / Excelencia** (basados en `ProgresoGeneral`).
  - Criterio exacto de **"Próx. a cerrar"** para competencias.
  - Umbral formal de **"baja asistencia"** (el diseño usa <70% pero no está en código como constante documentada).

### 7.9 Gestión de Aprendices

- Estados: `EN FORMACION`, `CANCELADO`, `TRASLADADO`, `RETIRO VOLUNTARIO`. **Valor canónico** → `TRASLADADO` (no `TRASLADO`).
- **En Riesgo** = supera el umbral de inasistencias (ver §7.6).
- El cambio de estado queda en **historial** (`HistorialEstadoAprendiz`: `EstadoAnterior`, `EstadoNuevo`, `IdUsuarioCambio`, `FechaCambio`) — **implementado**, se registra desde `AprendizRepository.EditarEstadoAsync`.
- Un aprendiz con estado ≠ `EN FORMACION` **no aparece** en listas de asistencia.
- **No se permite registro individual** por UI; todos deben importarse desde SOFIA Plus. El endpoint `POST /agregar-aprendiz` está reservado para escenarios administrativos excepcionales.
- La relación aprendiz–ficha se modela con la entidad `FichaAprendiz` (estado propio por ficha).

### 7.10 Gestión de Instructores

- El correo electrónico del instructor es **único** en el sistema.
- Política de contraseña: mínimo 8 caracteres, mayúscula, número, carácter especial — **implementado** en `PasswordPolicyValidator`, validado en `AuthController.CambiarPasswordPrimerLogin` y `PasswordController.CambiarPassword` antes de llamar al AppService.
- El instructor es forzado a cambiar contraseña en el primer inicio de sesión.
- Estado `Activo`/`Inactivo`. Un instructor `Inactivo` no puede iniciar sesión.
- El filtro por competencia muestra solo instructores con esa competencia asignada actualmente.

### 7.11 Repositorio y Gestión Documental

- La implementación real modela tipos documentales como entidades separadas: `GuiaAprendizaje` (con `Version` y `Estado` ∈ `Borrador/Publicado/Cerrado`), `InstrumentoEvaluacion`, `PlaneacionPedagogica`, `ProyectoFormativo`, `DesarrolloCurricular`, `PlanConcertado` (relacionado al aprendiz).
- La categoría **"Descargas"** es una vista derivada del historial de descargas del usuario.
- Cada documento se almacena en la entidad `Archivo` con referencias a `TipoArchivo`.
- **Formatos permitidos:** PDF, DOCX (para guías).
- **No hay versionamiento** de archivos a nivel de reemplazo: para modificar se elimina y recarga.
- **Permisos por rol:** Administrador (CRUD completo), Instructor (carga guías de sus competencias, ve todas, elimina solo las suyas — validado en `RepositorioAppService`), Aprendiz (solo lectura y descarga).
- **Almacenamiento: disco local vía `LocalArchivoStorageService`, de forma permanente** — se decidió **no** integrar SharePoint/Microsoft Graph (ver §4). Los campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` de `Archivo` quedan sin uso.

### 7.12 Juicios Evaluativos (núcleo académico SOFIA)

- `PlanConcertado` → plan formativo del aprendiz.
- `ResultadoAprendizaje` → resultado específico de una competencia.
- `JuicioResultado` → juicio (APROBADO / POR EVALUAR) emitido sobre un resultado para un aprendiz, con fecha y funcionario.
- Estas entidades son nucleares al flujo de SOFIA y alimentan la derivación de estados de competencia (§7.3, §7.5).

### 7.13 Reportes

- ✅ Implementado — `Reportes.razor` + `ReportesController` + `ReportesAppService`. 8 tipos de reporte: global, asistencia por ficha, progreso de competencias, juicios, por instructor, por competencia, patrocinio, informe individual (PDF).
- Construidos **en Blazor Server**, sin Power BI ni iframe.
- Cada instructor solo visualiza reportes de sus fichas — el filtrado se aplica en la API (`ObtenerIdUsuarioAutenticado()` + lógica interna del `AppService`), no solo en la UI.
- **Formatos de exportación:** PDF (jsPDF client-side, en `Informe.razor`) y Excel server-side (`InformeExcelGenerator`, 3 endpoints `/excel`) — ambos implementados. La descarga de Excel pasa por `HttpClient` autenticado + JS interop, **no** por link directo a la API (el navegador no tiene el token interno).

### 7.14 Auditoría y Trazabilidad

- La entidad `Auditoria` está definida con los campos estándar (`IdUsuario`, `Accion`, `TablaAfectada`, `DatosViejos`, `DatosNuevos`, `Fecha`, `DireccionIp`).
- **Implementado** vía `AuditoriaInterceptor` (`Infrastructure/Data/Interceptors/`, `SaveChangesInterceptor` de EF Core, inyectado con `IHttpContextAccessor` para resolver el `IdUsuario` del claim). Cubre `AprendizPerfil`/`InstructorPerfil` (Added/Modified/Deleted), `FichaAprendiz` (solo Modified — cambios de estado), `Archivo` (Added/Deleted) y `Usuario` (solo cuando `PasswordHash` cambia, para no generar ruido en cada login). Además se sigue registrando trazabilidad específica en `Usuario.UltimoAcceso`/`IntentosFallidos` e `ImportacionArchivo` + `ImportacionError`.

### 7.15 Monitorías

- Un aprendiz puede ser **monitor** de una competencia (no es un rol distinto — sigue siendo `Aprendiz`, con un `MonitorPerfil` asociado).
- Flujo: asignación de monitor por competencia → el monitor crea sesiones de monitoría → otros aprendices se inscriben → se registra asistencia e informe de la sesión.
- `MonitoriaController` sirve a los 3 roles a la vez (aprendices se inscriben a sí mismos, instructores/admin asignan monitores) — no tiene una página Blazor 1:1 de referencia para granularidad de rol fina; ver hallazgo en `README.md`.

---

## 8. Endpoints REST

El detalle completo de endpoints (verbo, ruta, rol requerido, si el id es SELF/LOOKUP) vive en [`README.md` § API REST — Controllers](README.md#api-rest--controllers) — no se duplica aquí para evitar que este documento se desactualice cada vez que cambie un endpoint. Regla general: **20 controllers** en `main`, todos bajo `[Authorize]` por defecto salvo login/activación/recuperación.

---

## 9. Comandos Útiles

```bash
# Compilar la solución completa
dotnet build AdsoLabs.sln

# Ejecutar la API
dotnet run --project AdsoLabs.API

# Ejecutar el Frontend
dotnet run --project AdsoLabs.Web

# Agregar migración
dotnet ef migrations add NombreMigracion --project AdsoLabs.Infrastructure --startup-project AdsoLabs.API

# Aplicar migraciones (el proyecto también las aplica automáticamente al arranque)
dotnet ef database update --project AdsoLabs.Infrastructure --startup-project AdsoLabs.API
```

Para configuración de entorno local (connection string, SMTP, Anthropic, `InternalAuth:Key`), ver [`README.md` § Primeros pasos](README.md#primeros-pasos).

---

## 10. Qué NO Tocar Sin Preguntar

- Archivos en `AdsoLabs.Infrastructure/Migrations/` — no modificar migraciones existentes manualmente.
- `limpiar_importacion.sql` en la raíz — script de limpieza de datos, solo usar bajo instrucción explícita.
- `appsettings.json` — no hardcodear connection strings ni secretos (van en `appsettings.Development.json`, local y gitignored).
- Lógica de importación SOFIA Plus (`ImportadorReporteJuicios`) — es atómica y compleja; modificar solo bajo petición expresa (ver §7.5).
- Módulo `AdsoLabs.Web/Pages/` (Razor Pages handlers) — son handlers de autenticación/activación/recuperación; no confundir con componentes Blazor.
- Esquema de hashing (`HashService` PBKDF2) — no cambiar algoritmo ni iteraciones sin plan de migración de contraseñas.
- `InternalAuth:Key` y el mecanismo de token interno Web↔API — no cambiar el esquema de autenticación de la API sin entender el impacto en los 19+ controllers que dependen de él.

---

## 11. Seguridad y Privacidad

- Manejo de datos personales sujeto a **Ley 1581 de Colombia (Habeas Data)**.
- Todo tráfico en HTTPS/TLS.
- Datos sensibles cifrados en reposo (TDE recomendado a nivel SQL Server).
- **No** incluir datos reales de aprendices en commits, logs ni ejemplos.
- El consentimiento informado se presenta en `PasswordChange.razor` (primer login) — implementado, ver §7.2. El texto del aviso (`wwwroot/docs/aviso-tratamiento-datos.html`) sigue siendo un **borrador pendiente de revisión legal**.
- Log de auditoría (ver §7.14) debe cubrir: creación/modificación de aprendices e instructores, cambios de estado, carga de archivos y cambios de contraseña.
- La API exige autenticación y rol en todos sus endpoints salvo los explícitamente públicos — ver §7.1 y §8.

---

## 12. Entidades del Dominio (resumen oficial)

**Catálogos:** `TipoDocumento`, `TipoArchivo`, `Rol`.
**Personas y seguridad:** `Persona`, `Usuario`, `PasswordResetToken`.
**Perfiles:** `AprendizPerfil`, `InstructorPerfil`, `MonitorPerfil`, `FichaAprendiz`.
**Fichas y competencias:** `Ficha`, `Competencia`, `FichaCompetencia`, `FichaCompetenciaResultado`, `HistorialEstadoAprendiz`, `Patrocinio`, `AsistenciaPatrocinio`.
**Asistencia:** `Sesion`, `Asistencia`.
**Monitorías:** `SesionMonitoria`, `InscripcionMonitoria`, `AsistenciaMonitoria`, `InformeMonitoria`, `InformeMonitoriaAprendiz`.
**Repositorio:** `Archivo`, `GuiaAprendizaje`, `InstrumentoEvaluacion`, `PlaneacionPedagogica`, `ProyectoFormativo`, `DesarrolloCurricular`.
**Juicios evaluativos:** `PlanConcertado`, `ResultadoAprendizaje`, `JuicioResultado`.
**Importaciones:** `ImportacionArchivo`, `ImportacionError`.
**Informe del aprendiz:** `Acudiente`, `ObservacionAprendiz`.
**Predicción IA:** `SolicitudAsignacion`.
**Notificaciones:** `Notificacion`.
**Auditoría:** `Auditoria`.

---

## 13. Historial de cambios del documento

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 4.2 | Jul 2026 | Se corrige el documento para reflejar el estado real tras la consolidación de PRs a `main`: Predicción IA migrada de Groq a **Anthropic Claude Haiku**; **Monitorías** pasa de "PR abierto" a ✅ Implementado (mergeado); **Auditoría transversal** pasa a ✅ Implementado (`AuditoriaInterceptor`); se agrega **Historial de Estado del Aprendiz** (✅, antes "pendiente"); se agrega **Política de contraseñas y consentimiento informado** (✅, antes pendiente); la lógica de "En Riesgo" en Asistencia pasa a implementada (`AsistenciaRiesgoCalculator`); se agrega **Fase formativa de competencias** como módulo nuevo; se corrige el conteo de controllers (20, no 19+1 pendiente). |
| 4.1 | Jul 2026 | Correcciones de alcance en §4: "Cronograma visual" se elimina como fila aparte — estaba desactualizada (arrastrada de versiones previas a RF-06), en realidad ya está implementada como parte de Perfil del Aprendiz (tab "Cronograma" en `Profile.razor`). **Integración SharePoint** y **Evidencias de Aprendizaje** pasan de "pendiente" a `🚫 Descartado` — decisión de producto confirmada, no van a implementarse; Repositorio Documental pasa a ✅ Implementado sin esa dependencia. Se agrega el símbolo `🚫 descartado` a la leyenda para distinguir "no implementado pero planeado" de "decidido fuera de alcance". |
| 4.0 | Jul 2026 | Se separa el contenido de proyecto (este archivo, `AGENTS.md`, tool-agnostic) del protocolo de comportamiento específico de Claude Code (`CLAUDE.md`, que ahora importa este archivo con `@AGENTS.md`). Se actualiza el estado real de módulos: Dashboard, Reportes y Perfil del Aprendiz pasan a ✅ Implementado; se agregan Monitorías (PR abierto), Predicción IA y Notificaciones. Se documenta la autorización real de la API (JWT interno Web↔API, `[Authorize(Roles=...)]` por controller, derivación SELF de `idUsuario`) reemplazando la nota de "sin autenticación" que ya no aplica. Se actualizan entidades del dominio y se elimina la tabla de endpoints duplicada en favor de `README.md`. |
| 3.3 | May 2026 | Decisión firme: se elimina Power BI. Dashboard y Reportes se implementan en Blazor Server con Chart.js/JSInterop. Se actualizan §3 (stack), §4 (módulos), §7.8 (diseño objetivo del dashboard), §7.13 (reportes Blazor). |
| 3.2 | May 2026 | Se agrega sección de protocolo de comportamiento del agente y router de skills (movido a `CLAUDE.md` en la v4.0). |
| 3.1 | Abr 2026 | Alineación con implementación real. Se elimina referencia a JWT (de cara al navegador) y se formaliza cookie auth + PBKDF2. |
| 3.0 | Feb 2026 | Versión previa; describía JWT + BCrypt como stack de autenticación (ya no aplicable). |

---

**Fin del documento**

*Este archivo es la referencia canónica de arquitectura y reglas de negocio para desarrolladores y agentes de IA. Cualquier nuevo módulo o regla debe reflejarse aquí antes de considerarse oficial.*
