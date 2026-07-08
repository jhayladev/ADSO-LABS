# ADSO Labs — Especificación de Requisitos de Software v3.4

**Sistema de Gestión del Programa ADSO**

---

| **Proyecto** | ADSO Labs |
|--------------|-----------|
| **Versión** | 3.4 — Reconciliación con `AGENTS.md` v4.2: SharePoint descartado (no pendiente), auditoría/asistencia-riesgo/política de contraseñas ya implementadas, nuevo módulo Fase Formativa |
| **Fecha** | Julio 2026 |
| **Clasificación** | Confidencial |
| **Metodología** | Scrum / Ágil |
| **Stack** | Blazor Server / .NET 9 / SQL Server |
| **Basado en** | SRS v3.3 + `AGENTS.md` v4.2 (fuente de verdad del proyecto, fusionada a `main` en PR #138) |

---

## Nota de versión 3.1

La versión 3.1 del SRS incorpora los hallazgos de la revisión técnica realizada en abril de 2026. Sus objetivos son:

- Marcar cada requisito con su **estado de cumplimiento** (✅ cumplido · ⚠️ parcial · ❌ no implementado).
- Ajustar la redacción para coincidir con lo implementado (especialmente autenticación y estados).
- Integrar formalmente los **nuevos requisitos RF-17 a RF-29** que cubren comportamiento ya presente en el código pero no documentado, incluyendo la formalización de los tres importadores (juicios SOFIA, aprendices y fichas) en RF-11, RF-28 y RF-29.
- **Eliminar** cualquier referencia a "Autenticación JWT" como requisito. La autenticación del sistema es por cookie (ASP.NET Core Authentication.Cookies); por tanto, el requisito que en revisiones previas se listaba como desviación técnica (JWT) **deja de considerarse tal**. Consecuentemente, toda mención a JWT en secciones de Stack, Reglas de Negocio o RNF ha sido reemplazada por la descripción real.

## Nota de versión 3.2

La versión 3.2 corrige desalineaciones detectadas entre la v3.1 y el estado real del código (verificado directamente, no solo por documentación):

- **Se elimina Power BI del sistema.** La v3.1 seguía tratándolo como "pendiente de configurar". Es una decisión de producto firme: el Dashboard y los Reportes se implementan íntegramente en **Blazor Server**, con gráficos vía **Chart.js/JSInterop**. Toda mención a Power BI, `reportId`, embed tokens o RLS de Power BI queda derogada.
- **RF-05 (Dashboard)** pasa de ⚠️ Parcial a ✅ Cumplido: existen los 4 endpoints `/api/mi-dashboard` (resumen, competencias, progreso-fichas, alertas) en `DashboardController`, consumidos por `Home.razor`. El `IdUsuario` se resuelve del token; no se requirió agregar claim `IdInstructor`.
- **RF-06 (Perfil del Aprendiz)** pasa de ❌ No implementado a ✅ Cumplido: `Profile.razor` está implementado (datos personales, acudiente, teléfono/dirección editables, cronograma y competencias).
- **RF-16 (Reportes)** se reescribe: ya no es un placeholder de Power BI. `Reportes.razor` está implementado en Blazor con exportación Excel server-side, filtrado por instructor en la capa de queries (`ReportesAppService` recibe `idUsuario` en cada método).
- Se ajustan en consecuencia RN-DASH, RN-REPORTES y RNF-04.

## Nota de versión 3.3

La v3.3 surge de una auditoría de cumplimiento en **Review Mode** (solo lectura) que comparó el inventario real de controladores, páginas Blazor y entidades contra el SRS v3.2. Se detectaron **tres módulos completos implementados y no documentados**, y dos requisitos marcados como pendientes que ya estaban resueltos:

- **Nuevo RF-30 — Monitorías:** módulo de aprendiz-monitor (asignación, sesiones, inscripciones, asistencia, informes). `MonitoriaController`, `MonitoriaAppService`, `MonitoriasAdmin.razor`.
- **Nuevo RF-31 — Predicción de Compatibilidad con IA:** integra la **API de Anthropic Claude** para predecir compatibilidad instructor-competencia y desempeño de aprendizaje. `PrediccionIAController`, `AiAnalysisService`, `PrediccionIA.razor`.
- **Nuevo RF-32 — Notificaciones en Tiempo Real:** bandeja de notificaciones vía SignalR (`NotificacionHub`) además de API REST (`NotificacionController`).
- **RF-13/RF-20 (Repositorio Documental)** corregido de ⚠️ Parcial a ✅ Cumplido: `Repositorio.razor` y `Guias.razor` existen y `RepositorioController` expone ~30 endpoints funcionales. (La v3.4 corrige además que SharePoint no es "pendiente" sino 🚫 descartado — ver Nota de versión 3.4.)
- **RN-APRENDIZ-03 (Historial de Estado)** corregido de ❌ a ✅: `HistorialEstadoAprendiz` ya existe como entidad.
- Se documentan las entidades nuevas: `Acudiente`, `ObservacionAprendiz`, `AsistenciaPatrocinio`, `Notificacion`, `MonitorPerfil`, `SesionMonitoria`, `InscripcionMonitoria`, `AsistenciaMonitoria`, `InformeMonitoria`, `InformeMonitoriaAprendiz`, `SolicitudAsignacion` (Anexo B).
- Se aclara en §2.1 (Stack) y RN-AUTH-09 la autenticación **interna** Web→API (JWT corto de servidor a servidor, distinto de la sesión del usuario que sigue siendo 100% cookie).
- Se añade §8.1 (Seguridad y Privacidad) con una advertencia formal sobre el envío de datos de aprendices/instructores a un proveedor de IA externo (Anthropic) en el módulo de Predicción IA, con implicación directa en Ley 1581.
- **Nota fuera de alcance:** la tabla de endpoints por controlador vive en `README.md`, no en este SRS.

## Nota de versión 3.4

La v3.4 corrige la v3.3 tras contrastarla contra `AGENTS.md` v4.2 — el documento vivo de arquitectura/reglas de negocio del proyecto, que se fusionó a `main` en paralelo a este trabajo (PR #138 y relacionados). La v3.3 seguía arrastrando varios "pendientes" que en realidad ya estaban resueltos o **descartados por decisión de producto** (que no es lo mismo que "pendiente"):

- **SharePoint queda 🚫 Descartado, no pendiente.** Es una decisión de producto firme: el repositorio documental usa almacenamiento en disco local (`LocalArchivoStorageService`) **de forma permanente**, no como paso intermedio hacia SharePoint. Se corrige en Stack (§2.1), RF-13, RF-20, RN-REPO-10, §1.2, §1.3 y el diagrama de contexto (Anexo A). Los campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` en `Archivo` quedan como legado sin uso.
- **RN-ASIST-03 / RN-APRENDIZ-02 / RN-APRENDIZ-05 (En Riesgo):** pasan de ⚠️ a ✅ — `AsistenciaRiesgoCalculator` ya implementa la regla completa (3 inasistencias consecutivas o 20% de horas), reutilizada de forma consistente entre Aprendiz, Informe y Reportes.
- **RN-AUDIT-02 / RNF-07 (Auditoría transversal):** pasan de ❌/⚠️ a ✅ — `AuditoriaInterceptor` (EF Core `SaveChangesInterceptor`) ya cubre `AprendizPerfil`, `InstructorPerfil`, `FichaAprendiz` (cambios de estado), `Archivo` y `Usuario.PasswordHash`.
- **RN-INSTRUCTOR-02 / RN-ACTIV-07 (política de contraseña):** pasan de ❌ a ✅ — `PasswordPolicyValidator` (mínimo 8 caracteres, mayúscula, número, carácter especial) ya se valida en `AuthController` y `PasswordController`.
- **Consentimiento informado (RNF-02):** pasa de "pendiente" a ✅ implementado — checkbox obligatorio en `PasswordChange.razor`, validado también en servidor. Se mantiene como pendiente **únicamente** la revisión legal del texto del aviso de tratamiento de datos (`wwwroot/docs/aviso-tratamiento-datos.html`), que sigue siendo borrador.
- **§2.3 (Regla de dependencias):** se corrige de `Web → Application → Core ← Infrastructure` a `Web → API → Application → Core ← Infrastructure` — `Web` ya no consume `Application` directamente, todo pasa por `AdsoLabs.API` vía `HttpClient`.
- **Nuevo RF-33 — Fase Formativa de Competencias:** cada competencia del catálogo tiene una fase (Inducción/Análisis/Planeación/Ejecución/Evaluación/Transversal), visible en Fichas, Asignación de Competencias y la pestaña "Ruta Formativa" (Kanban) de Fichas. Módulo no documentado en versiones previas.
- **RF-31 (Predicción IA):** se precisa el modelo real — Anthropic **Claude Haiku** (`claude-haiku-4-5-20251001`), migrado desde una integración previa con Groq/LLaMA 3.3.
- **HU-08 y HU-14** (Historias de Usuario): se retira la marca ⚠️ parcial — ambas están resueltas (informe PDF con datos reales, repositorio con UI completa).

---

## Tabla de Contenidos

1. Introducción
2. Arquitectura del Sistema
3. Requisitos Funcionales (RF-02 a RF-16 + RF-17 a RF-33)
4. Historias de Usuario
5. Flujos de Usuario Detallados
6. Reglas de Negocio Consolidadas
7. Requisitos No Funcionales
8. Seguridad y Privacidad
9. Glosario Técnico
10. Anexos

---

## 1. Introducción

### 1.1 Propósito del Documento

Este documento constituye la versión 3.1 de la Especificación de Requisitos de Software (SRS) para ADSO Labs. Reemplaza a la v3.0 y consolida:

- Los requisitos funcionales, reglas de negocio y requisitos no funcionales heredados.
- El estado real de cumplimiento de cada requisito, con base en la revisión técnica de la implementación.
- Los nuevos requisitos RF-17 a RF-29 derivados de funcionalidades presentes en el código.
- Los ajustes de redacción necesarios para que la especificación coincida fielmente con el sistema construido.

### 1.2 Alcance del Sistema

ADSO Labs es una plataforma web de gestión académica para el programa **Análisis y Desarrollo de Software Orientado (ADSO)** del SENA que centraliza:

- ✅ Gestión de fichas de formación y competencias
- ✅ Control de aprendices, instructores y asistencia
- ✅ Repositorio digital de guías y documentos académicos (almacenamiento en disco local; SharePoint 🚫 descartado por decisión de producto)
- ✅ Gestión de patrocinios
- ✅ Dashboard y Reportes analíticos en Blazor Server (Chart.js), sin Power BI
- ✅ Importación automatizada desde SOFIA Plus
- ✅ Monitorías académicas (aprendiz-monitor)
- ✅ Predicción de compatibilidad y desempeño con IA generativa (Anthropic Claude Haiku)
- ✅ Notificaciones en tiempo real (SignalR)
- ✅ Fase formativa de competencias (Ruta Formativa)

### 1.3 Stakeholders y Roles

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| **Administrador** | Gestión completa del sistema | CRUD total, importación SOFIA, gestión usuarios, reportes globales |
| **Instructor** | Gestión pedagógica de sus fichas | Gestión competencias asignadas, asistencia, guías, aprendices de sus fichas |
| **Aprendiz** | Consulta de información académica | Lectura de perfil/competencias, descarga guías, edición de contacto; puede ser monitor de una competencia. Subida de evidencias: 🚫 descartada, no forma parte del alcance |

---

## 2. Arquitectura del Sistema

### 2.1 Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| **Frontend** | Blazor Server | .NET 9 |
| **Backend API** | ASP.NET Core Web API | .NET 9 |
| **ORM** | Entity Framework Core | 9.x |
| **Base de Datos** | Microsoft SQL Server | 2019+ |
| **Autenticación (usuario)** | **ASP.NET Core Authentication.Cookies** (esquema `AdsoLabs.Auth`, HttpOnly + Secure, 8h sliding) | .NET 9 |
| **Autenticación interna (Web→API)** | JWT Bearer de corta duración, servidor a servidor: `AdsoLabs.Web` emite el token a partir de la cookie ya validada; el navegador nunca lo recibe. No sustituye ni convive con JWT del lado del usuario (ver RN-AUTH-09) | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| **Hashing de contraseñas** | **PBKDF2-SHA256 (100.000 iteraciones, salt 16 bytes, hash 32 bytes)** | .NET Core Cryptography |
| **Email** | MailKit / MimeKit | Última estable |
| **Importación Excel** | ExcelDataReader | 3.x |
| **Dashboard / Reportes** | Blazor Server + Chart.js vía JSInterop; exportación Excel server-side | .NET 9 |
| **IA Generativa** | API de Anthropic Claude (`https://api.anthropic.com/v1/messages`), consumida por `AiAnalysisService` para el módulo de Predicción IA (RF-31) | API Anthropic 2023-06-01 |
| **Tiempo real** | SignalR (`NotificacionHub`) para notificaciones (RF-32) | ASP.NET Core SignalR |
| **UI Framework** | Bootstrap 5 | 5.3+ |
| **Almacenamiento Archivos** | Disco local vía `LocalArchivoStorageService`, **de forma permanente** — decisión de producto firme, no un paso intermedio. Integración SharePoint/Microsoft Graph: 🚫 **descartada** (no es un pendiente técnico). Los campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` en `Archivo` quedan sin uso, legado del diseño original | — |

> Las versiones anteriores del SRS mencionaban JWT + BCrypt; la implementación real usa cookie auth + PBKDF2-SHA256. Esta v3.1 oficializa el stack real.

### 2.2 Arquitectura en Capas (Clean Architecture)

```
AdsoLabs.sln
├── AdsoLabs.Core
│   ├── Entities/
│   ├── Enums/
│   ├── Interfaces/ (contratos específicos de dominio)
│   └── ValueObjects/
│
├── AdsoLabs.Application
│   ├── DTOs/              (por módulo)
│   ├── DTOs/Commands/     (Agregar*Command)
│   ├── Services/          (AppServices)
│   └── Interfaces/Services/
│
├── AdsoLabs.Infrastructure
│   ├── Data/              AdsoDbContext
│   ├── Migrations/        17 migraciones
│   ├── Repositories/
│   ├── Services/          Auth, ActivacionCuenta, RecuperacionPassword, Email,
│   │                      ImportadorReporteJuicios, ImportadorAprendices, ImportadorFichas
│   └── Security/          HashService (PBKDF2)
│
├── AdsoLabs.API
│   ├── Controllers/
│   ├── Middleware/
│   └── Configuration/     (DI, CORS, auto-migración, seeder)
│
└── AdsoLabs.Web
    ├── Components/        (Pages y Layout Blazor)
    ├── Pages/             (Razor Pages handlers)
    └── Services/Api/      (*ApiService)
```

### 2.3 Regla Fundamental de Dependencias

```
Web → API → Application → Core ← Infrastructure
```

> Corrección v3.4: la v3.1 documentaba `Web → Application → Core ← Infrastructure`, implicando que `Web` consumía `Application` directamente. Eso no refleja la implementación real: `Web` **no** llama a servicios externos ni a `Application` directamente — consume exclusivamente `AdsoLabs.API` vía `HttpClient` (un `*ApiService.cs` por módulo).

**Restricciones:**
- `Core` NO depende de ninguna otra capa. Solo contiene entidades base de identidad (`Persona`, `Usuario`, `Rol`, `TipoDocumento`, `Ficha`, `Competencia`, `FichaCompetencia`, `AprendizPerfil`, `InstructorPerfil`, `PasswordResetToken`) y enums de resultado.
- `Infrastructure` implementa interfaces definidas en `Application` (servicios y repositorios).
- `Web` y `API` consumen exclusivamente `Application`; nunca `Infrastructure` ni entidades de `Core` directamente. `Web` tampoco llama a servicios externos directamente: todo pasa por `AdsoLabs.API`.

---

## 3. Requisitos Funcionales

> **Leyenda de estado:** ✅ cumplido · ⚠️ parcial · ❌ no implementado.
> **Nota:** En esta v3.1 **no existe RF-01**. El identificador fue retirado tras eliminar el requisito "Autenticación JWT"; la autenticación del sistema queda descrita en RF-02 (Bloqueo) y en la sección 2.1 (Stack) como cookie auth. Se conservan los identificadores RF-02 a RF-29 para evitar renumeración de referencias cruzadas.

### 3.1 Módulo de Autenticación

#### RF-02: Inicio de Sesión y Control de Intentos Fallidos

| Campo | Detalle |
|-------|---------|
| **ID** | RF-02 |
| **Prioridad** | Alta |
| **Actores** | Instructor, Aprendiz, Administrador, Sistema |
| **Estado** | ✅ Cumplido |

**Descripción:**
El sistema autentica usuarios mediante correo y contraseña, emite cookie de sesión (`AdsoLabs.Auth`) con claims de rol, y previene fuerza bruta mediante bloqueo por intentos fallidos.

**Flujo Principal:**
1. Usuario envía `POST /login` con correo y contraseña.
2. Sistema busca al usuario por correo; si no existe, responde "Usuario o contraseña incorrectos" (sin enumeración).
3. Si `Usuario.Activo = false` → mismo mensaje genérico.
4. Si `PrimerLogin = true` → compara contraseña contra número de documento.
5. Si `PrimerLogin = false` → compara hash PBKDF2-SHA256 con salt del usuario.
6. Si válido → emite cookie, actualiza `UltimoAcceso` y redirige según rol.
7. Si `PrimerLogin = true` → fuerza cambio de contraseña (`POST /cambiar-password-primer-login`).

**Parámetros de Bloqueo:**
- `IntentosMaximos = 5`
- `VentanaTiempo = 15 minutos`
- `DuracionBloqueo = 30 minutos`

**Excepciones:**
- **Administradores:** nunca se bloquean.
- **Instructores y Aprendices:** se bloquean según la regla.

**Auditoría:**
- `Usuario.IntentosFallidos` incrementa con cada fallo.
- `Usuario.UltimoAcceso` se actualiza en login exitoso.
- `Usuario.BloqueadoHasta` almacena el fin del bloqueo.

**Reglas de Negocio Aplicables:** RN-AUTH-02 a RN-AUTH-08.

---

#### RF-03: Activación de Cuenta

| Campo | Detalle |
|-------|---------|
| **ID** | RF-03 |
| **Prioridad** | Alta |
| **Estado** | ✅ Cumplido |
| **Actores** | Aprendiz, Instructor |

Permite a usuarios ya registrados (`Persona` + perfil sin `IdUsuario`) crear credenciales. Flujo: documento + correo → creación `Usuario (Activo=false, PrimerLogin=true)` → envío de token GUID hasheado SHA256 con expiración 30 min → clic en enlace → `Activo=true` → primer login con documento como contraseña → cambio de contraseña forzado.

**Reglas:** RN-ACTIV-01 a RN-ACTIV-06.

---

#### RF-04: Recuperación de Contraseña

| Campo | Detalle |
|-------|---------|
| **ID** | RF-04 |
| **Prioridad** | Media |
| **Estado** | ✅ Cumplido |
| **Actores** | Instructor, Aprendiz |

Reutiliza `PasswordResetToken`. Actualiza `PasswordHash`/`PasswordSalt` sin tocar `Activo` ni `PrimerLogin`. Cooldown de 5 minutos.

---

### 3.2 Módulo de Dashboard y Perfil

#### RF-05: Dashboard Principal del Instructor

| Campo | Detalle |
|-------|---------|
| **ID** | RF-05 |
| **Prioridad** | Alta |
| **Estado** | ✅ Cumplido |
| **Actores** | Instructor, Administrador |

**Descripción:**
Panel centralizado con métricas, alertas y accesos rápidos, construido íntegramente en Blazor Server. **No usa Power BI** (decisión de producto derogó el requisito original).

**Implementación actual:**
- `DashboardController` expone `GET /api/mi-dashboard` (resumen de cabecera), `/competencias` (donut de estados), `/progreso-fichas` (horas planeadas vs ejecutadas) y `/alertas` (baja asistencia, retraso de horas, al día, culminan esta semana). El `IdUsuario` se resuelve del token autenticado, no se acepta como parámetro del cliente.
- `Home.razor` consume estos endpoints y renderiza los gráficos con Chart.js vía JSInterop.
- El gap de `IdInstructor` señalado en revisiones previas se resolvió sin agregar claim: el filtrado se hace en el `AppService`/query a partir de `IdUsuario`.

**Reglas:** RN-DASH-01 a RN-DASH-03.

---

#### RF-06: Perfil del Aprendiz

| Campo | Detalle |
|-------|---------|
| **ID** | RF-06 |
| **Prioridad** | Alta |
| **Estado** | ✅ Cumplido |
| **Actores** | Aprendiz |

Vista con datos personales (solo lectura excepto teléfono y dirección), datos del acudiente, cronograma y competencias de la ficha.

**Estado real:** `Profile.razor` está implementado con las pestañas Mis Datos, Acudiente, Cronograma y Competencias. Teléfono y dirección son editables; el resto de datos personales es de solo lectura. **Fuera de alcance — 🚫 Descartado** (decisión de producto, no un pendiente): integración SharePoint y carga de evidencias de aprendizaje — no forman parte de este requisito.

---

### 3.3 Módulo de Gestión de Fichas

#### RF-07: Listado de Fichas en Menú Lateral

| Campo | Detalle |
|-------|---------|
| **Estado** | ✅ Cumplido |

`NavMenu.razor` consume `GET /obtener-fichas`. Formato `[Número] - [Programa]`, ordenado ascendentemente. Filtrado por rol.

---

#### RF-08: Pantalla de Detalle de Ficha

| Campo | Detalle |
|-------|---------|
| **Estado** | ✅ Cumplido |

**Implementado:** tabs Aprendices y Competencias, encabezado con métricas, botón Informe, y una tercera pestaña **"Cronograma" (Kanban por fase formativa)** en `Fichas.razor` — corregido en v3.4: lo que la v3.1 documentaba como "tab Cronograma pendiente" (vista de calendario con conflictos de horario) se implementó finalmente como Kanban de "Ruta Formativa" agrupado por fase (ver RF-33), no como calendario de conflictos de horario. Si el calendario de conflictos de horario sigue siendo un requisito de negocio distinto, queda como pregunta abierta para el equipo.

---

#### RF-09: Asignación y Programación de Competencias

| Campo | Detalle |
|-------|---------|
| **Estado** | ✅ Cumplido |

La programación se realiza por `FichaCompetenciaResultado` con `IdInstructor`, `Fecha`, `HoraInicio`, `HoraFin`, `Horas` y `Estado ∈ {Pendiente, Programado, Completado}`. Validaciones RN-PROG-01 a RN-PROG-09 aplicadas.

---

### 3.4 Módulo de Gestión de Competencias

#### RF-10: Catálogo de Competencias — ✅ Cumplido

Listado con métricas (Total / Activas / Clausuradas) y filtros. Lógica de "Clausurada visible pero no asignable" está soportada por datos.

---

### 3.5 Módulo de Gestión de Aprendices

#### RF-11: Importación del Reporte de Juicios Evaluativos (SOFIA Plus)

| Campo | Detalle |
|-------|---------|
| **ID** | RF-11 |
| **Prioridad** | Alta |
| **Actores** | Administrador, Sistema |
| **Estado** | ✅ Cumplido |
| **Componente** | `ImportadorReporteJuicios` (`AdsoLabs.Application.Importacion`) |
| **Endpoint** | `POST /importar-reporte` (multipart `.xls` + `idUsuario`) |

**Descripción:**
El sistema procesa el archivo `.xls` "Reporte de Juicios Evaluativos" exportado desde SOFIA Plus, sincronizando estados de aprendices, competencias y resultados de aprendizaje en la ficha correspondiente. La importación NO crea fichas ni aprendices: refleja la realidad del reporte sobre datos ya cargados.

**Arquitectura en dos fases (desacopladas):**

| Fase | Método | Entrada → Salida | Características |
|------|--------|------------------|-----------------|
| **Fase 1 — Normalización** | `NormalizarReporteSofia(Stream)` | Excel SOFIA → `List<ReporteJuicioDto>` | Función pura, sin EF Core ni BD. Detecta dinámicamente la fila de encabezado y mapea columnas por nombre. Emite TODAS las filas con juicio (`APROBADO` y `POR EVALUAR`). Solo descarta filas vacías o sin juicio. |
| **Fase 2 — Persistencia** | `ImportarNormalizadoAsync(dtos, idUsuario, idArchivo)` | `List<ReporteJuicioDto>` → BD | Upserts idempotentes, una transacción por DTO, recálculo final de estados de `FichaCompetencia`. |

**Lectura del Excel (Fase 1):**
- La fila de encabezados se **localiza dinámicamente** buscando una fila que contenga simultáneamente "TIPO", "DOCUMENTO" y "NOMBRE" en las primeras 20 filas. Si no se encuentra, se asume la fila 9 (0-based, posición histórica de SOFIA Plus).
- Los metadatos de la cabecera (`NumeroFicha`, `Denominacion`, `EstadoFicha`, `FechaInicio`, `FechaFin`, `Modalidad`, `Regional`, `Centro`) se extraen escaneando las primeras 15 filas por palabras clave (tolerante a variantes ortográficas como `COGIGO`/`CÓGIGO`/`CODIGO`).
- Las columnas se mapean por nombre con tolerancia (`TIPO`, `NÚMERO DE DOC`, `NOMBRE`, `APELLIDO`, `ESTADO`, `COMPETENCIA`, `RESULTADO`, `JUICIO`, `FECHA`, `FUNCIONARIO`); valores por defecto históricos cuando no se localizan.
- El número de ficha se extrae adicionalmente de la celda `(2,2)` (posición fija histórica) usando regex de 5+ dígitos.
- Las filas con `Documento` y `Competencia` ambos vacíos, o con `JuicioEvaluativo` vacío, se descartan silenciosamente.

**Validación global previa a la persistencia:**
- La **ficha indicada en el Excel debe existir** en BD. Si no existe, se rechaza TODO el archivo, se marca `ImportacionArchivo.EstadoProcesamiento = Error`, y se registra el motivo en `Errores`. La creación de fichas es responsabilidad del módulo de importación de fichas (RF-29), no del importador de juicios.

**Procesamiento por DTO (cada uno en su propia transacción):**

1. **Detección de Etapa Práctica/Productiva** — si `CodigoCompetencia == "590803"` o el nombre contiene `ETAPA PRACTICA` / `ETAPA PRODUCTIVA`, se omite todo el procesamiento académico (competencia, ficha-competencia, plan, resultado, juicio); solo se actualiza el estado del aprendiz si corresponde.
2. **Upsert `Competencia`** — si el código no existe, se crea con un tipo inferido (`Practica`, `Induccion`, `Clave`, `Transversal`, `Tecnica`) según el código y `HorasAsignadas = 48` por defecto.
3. **Upsert `FichaCompetencia`** — vincula la competencia con la ficha en estado `Pendiente`. `FechaInicio`, `FechaFin` y `TotalHoras` quedan en `NULL` hasta configuración manual.
4. **Upsert `PlanConcertado`** — uno por competencia, sin instructor por defecto.
5. **Upsert `ResultadoAprendizaje`** — clave compuesta `(IdPlan, Codigo)`.
6. **Upsert `FichaCompetenciaResultado` (FCR)** — se crea como `Pendiente` si no existe. Se evalúa si tiene programación activa (`Programado` o `Completado`).
7. **Búsqueda de aprendiz (sin crear)** — si el documento no está registrado en `AprendizPerfil`, la fila se rechaza con error registrado en `ImportacionError`; las demás filas continúan. La creación de aprendices es responsabilidad del módulo de carga de aprendices (RF-28).
8. **Actualización de estado del vínculo `FichaAprendiz`** — solo si el vínculo ya existe; en caso contrario no se crea. Aplica la regla de traslado automático (no revierte si existe ficha más reciente).
9. **`JuicioResultado`** — solo se persiste para juicios `APROBADO` **y** cuando el FCR está `Programado` o `Completado`. Si el FCR está `Pendiente`, el juicio se omite silenciosamente (no es error). Las filas `POR EVALUAR` solo materializan el FCR (paso 6) y actualizan el estado del aprendiz (paso 8); su ausencia en `JuicioResultado` es semánticamente equivalente a "aún sin aprobar".

**Atomicidad:**
- **Por fila, no global.** Cada DTO se procesa en su propia transacción EF Core. Un error en una fila hace rollback de esa fila y registra el error en `ImportacionError`, pero NO detiene el procesamiento de las demás filas. La excepción es la validación global de existencia de ficha, que sí rechaza todo el archivo.

**Caches en memoria con eviction selectiva:**
- Se mantienen caches por archivo (Competencia, FichaCompetencia, Plan, Resultado, Aprendiz) para evitar SELECTs repetidos. En rollback de una fila, solo se evictan las entradas insertadas en esa iteración (`isNew = true`); las entradas resueltas vía `SELECT` permanecen porque siguen siendo válidas en BD.

**Recálculo final de `FichaCompetencia` (RN-SOFIA-05/06/07):**
- Una vez procesados todos los DTOs, se recalcula el estado de cada `FichaCompetencia` de la ficha basándose en el estado de sus FCRs:
  - **`Vista`:** todos los FCRs están en `Completado`. Estado máximo — **nunca se degrada** por reimportación.
  - **`En Curso`:** ≥1 FCR en `Completado` o ≥1 FCR en `Programado` (pero no todos `Completado`).
  - **`Pendiente`:** 0 FCRs `Completado` y 0 FCRs `Programado`.
- Un FCR transita a `Completado` cuando todos los aprendices `EN FORMACION` tienen `JuicioResultado = APROBADO` para ese resultado. La transición preserva los campos de programación (instructor, fechas, horas).
- `HorasEjecutadas` se recalcula como la suma de horas programadas de los FCRs `Completado` (o `TotalHoras` si la competencia entra en `Vista`).

**Trazabilidad:**
- Toda importación crea un registro en `ImportacionArchivo` (cabecera con `IdUsuario`, `TotalFilas`, `EstadoProcesamiento`, `FechaProcesado`, `Observacion = "Ficha {N} — Juicios Evaluativos SOFIA Plus"`).
- Cada error por fila se persiste en `ImportacionError` (`NumeroFila`, `DatosFila` JSON-serializado, `MotivoError` truncado a 490 caracteres).
- Estado final: `Procesado` si `FilasError == 0`, `Error` en caso contrario.

**Resultado devuelto (`ResultadoImportacion`):**
- `IdImportacion`, `TotalFilas`, `FilasOk`, `FilasError`, `JuiciosInsertados`, `JuiciosActualizados`, `FichasAfectadas`, `Errores[]`.

**Reglas:** RN-SOFIA-01 a RN-SOFIA-13.

---

#### RF-12: Cambio de Estado de Aprendices

| Estado | ✅ Cumplido |
|--------|-------------|

El cambio se realiza mediante `PUT /{idAprendiz}/estado` y sincroniza `Usuario.Activo`. La entidad `HistorialEstadoAprendiz` ya existe (`AdsoLabs.Infrastructure/Models/HistorialEstadoAprendiz.cs`) — la v3.1 la listaba como pendiente; queda corregido en v3.3.

**Estados válidos:** `EN FORMACION`, `CANCELADO`, `TRASLADADO`, `RETIRO VOLUNTARIO`. Se canoniza `TRASLADADO` como forma oficial (v3.0 alternaba con `TRASLADO`).

---

### 3.6 Módulo de Repositorio

#### RF-13: Gestión de Guías de Aprendizaje

| Estado | ✅ Cumplido |
|--------|-------------|

> Corrección v3.3: la v3.1/v3.2 marcaban este RF como ⚠️ Parcial con "UI pendiente". Se verificó que `Repositorio.razor` (850 líneas) y `Guias.razor` (307 líneas) existen y funcionan, respaldados por `RepositorioController` con ~30 endpoints (listar, subir, descargar, cambiar estado, eliminar, selectores por ficha/competencia) para los 6 tipos documentales.
> Corrección v3.4: la integración SharePoint/Graph **no es un pendiente**, es 🚫 **descartada** por decisión de producto — el repositorio funciona con almacenamiento en disco local de forma permanente (`LocalArchivoStorageService`).

La v3.0 planteaba "categorías fijas" en un único modelo. La implementación separa los tipos documentales en entidades distintas:

| Tipo | Entidad |
|------|---------|
| Guía de Aprendizaje | `GuiaAprendizaje` (con `Version`, `Estado ∈ {Borrador, Publicado, Cerrado}`) |
| Instrumento de Evaluación (reutilizable) | `InstrumentoEvaluacion` |
| Planeación Pedagógica | `PlaneacionPedagogica` |
| Proyecto Formativo | `ProyectoFormativo` |
| Desarrollo Curricular | `DesarrolloCurricular` |
| Plan Concertado / Plan de Mejoramiento | `PlanConcertado` |
| Descargas | vista derivada del historial de descargas (`GET /api/repositorio/mis-descargas`) |

**Almacenamiento:** disco local vía `LocalArchivoStorageService`, de forma permanente. Los campos `SiteId`/`DriveId`/`ItemId`/`WebUrl` de `Archivo` son legado del diseño original y no se usan.

Reglas: RN-REPO-02, RN-REPO-04, RN-REPO-05, RN-REPO-07, RN-REPO-08, RN-REPO-09 (aplican, ver corrección en §6). RN-REPO-10 (SharePoint) → 🚫 **descartado**.

---

### 3.7 Módulo de Asistencia

#### RF-14: Registro de Asistencia

| Estado | ⚠️ Parcial |
|--------|-------------|

**Implementado:** entidades `Sesion` (Fecha, HoraInicio, HoraFin) y `Asistencia` (Estado ∈ `Presente/Justificado/Ausente`). UI en `Asistencias.razor` y `AsistenciasPatrocinado.razor`. Detección de **"En Riesgo"** completa (3 inasistencias consecutivas **o** 20% de horas por competencia) vía `AsistenciaRiesgoCalculator`, única fuente de verdad reutilizada por Aprendiz, Informe y Reportes — corregido en v3.4 (RN-ASIST-03 pasa de ⚠️ a ✅).

**Pendiente (esto sí sigue siendo un pendiente técnico, no una decisión descartada):**
- Inmutabilidad de registros confirmados (RN-ASIST-04). La entidad es mutable.
- Mecanismo de "corrección por administrador" con auditoría (RN-ASIST-05).

---

### 3.8 Módulo de Patrocinio

#### RF-15: Gestión de Horarios de Patrocinio

| Estado | ✅ Cumplido |
|--------|-------------|

Entidad `Patrocinio` con `Etapa ∈ {Lectiva, Productiva}`, `FechaInicioEtapa`, `FechaFinEtapa`, `NombreEmpresa` y `ContactoEmpresa`. Submódulo `AsistenciasPatrocinado.razor` específico.

Reglas RN-PATROC-01 a RN-PATROC-04 aplicadas.

---

### 3.9 Módulo de Reportes

#### RF-16: Reportes Analíticos en Blazor Server

| Estado | ✅ Cumplido |
|--------|-------------|

> Requisito reescrito: la versión original (Power BI Embebido) fue derogada por decisión de producto. Se conserva el ID RF-16 para no romper referencias cruzadas.

`Reportes.razor` implementado con pestañas de reporte global, asistencia por ficha, progreso de competencias y patrocinios. Exportación a Excel server-side (botones "Exportar Excel" por sección, descarga vía `ReportesApiService`). El filtrado por instructor se aplica en `ReportesAppService`/`IReportesQueryService`, que reciben `idUsuario` en cada método (no solo en la UI).

---

### 3.10 Nuevos Requisitos (v3.1)

Los siguientes requisitos formalizan funcionalidades ya implementadas en el código que no figuraban en v3.0.

#### RF-17: Trazabilidad de Importaciones SOFIA Plus

| Campo | Detalle |
|-------|---------|
| **Estado** | ✅ Cumplido |
| **Actores** | Administrador, Sistema |

El sistema registra en `ImportacionArchivo` cada importación (usuario, archivo, fecha, estado), y en `ImportacionError` cada error por fila (fila, tipo de error, valor inválido, mensaje). Estas tablas son consultables para auditoría y diagnóstico.

---

#### RF-18: Sesiones Académicas (Sesion)

| Estado | ✅ Cumplido |
|--------|-------------|

La unidad base para la asistencia es la entidad `Sesion`, asociada a `FichaCompetenciaResultado`, con `Fecha`, `HoraInicio`, `HoraFin`. Cada sesión contiene múltiples registros de `Asistencia` por aprendiz.

---

#### RF-19: Modelado de Juicios Evaluativos

| Estado | ✅ Cumplido |
|--------|-------------|

Se modela la estructura del Reporte de Juicios Evaluativos de SOFIA Plus mediante:

- `PlanConcertado` — plan formativo del aprendiz.
- `ResultadoAprendizaje` — componente de una competencia.
- `JuicioResultado` — juicio (`APROBADO` | `POR EVALUAR`) emitido sobre un resultado para un aprendiz, con `FechaJuicio` y `FuncionarioRegistro`.

Estas entidades alimentan la derivación de estados de competencia (Vista / En Curso / Pendiente).

---

#### RF-20: Repositorio Documental Ampliado

| Estado | ✅ Cumplido |
|--------|-------------|

Además de `GuiaAprendizaje`, se modelan los tipos `InstrumentoEvaluacion` (reutilizable entre competencias y fichas), `PlaneacionPedagogica`, `ProyectoFormativo` y `DesarrolloCurricular`. Cada uno tiene autor y asociaciones específicas. `Repositorio.razor` está implementado (ver RF-13). La integración con SharePoint es 🚫 **descartada** (decisión de producto), no un pendiente.

---

#### RF-21: Relación Ficha–Aprendiz (FichaAprendiz)

| Estado | ✅ Cumplido |
|--------|-------------|

La relación entre ficha y aprendiz se modela con la entidad `FichaAprendiz` con `Estado` propio por ficha. Un aprendiz puede existir en el catálogo `AprendizPerfil` y mantener distintos estados por ficha (histórico de permanencia).

---

#### RF-22: Catálogo de Tipos de Archivo (TipoArchivo)

| Estado | ✅ Cumplido |
|--------|-------------|

La entidad `TipoArchivo` parametriza los formatos permitidos en el repositorio documental y las evidencias. Permite extender sin cambios de código los formatos aceptados.

---

#### RF-23: Patrocinio Empresarial

| Estado | ✅ Cumplido |
|--------|-------------|

La entidad `Patrocinio`, además de los campos descritos en RF-15, almacena los datos de la empresa patrocinadora: `NombreEmpresa` y `ContactoEmpresa`. Estos datos se usan para reportes de seguimiento y gestión de la etapa productiva.

---

#### RF-24: Endpoint de Diagnóstico de Correo

| Estado | ✅ Cumplido |
|--------|-------------|

`EmailController` expone `POST /probar` para verificar la configuración SMTP sin afectar flujos funcionales. Herramienta operativa para validación de despliegue.

---

#### RF-25: Resúmenes Agregados (Dashboard)

| Estado | ✅ Cumplido |
|--------|-------------|

`AprendizController.obtener-resumen-aprendices` e `InstructorController.obtener-resumen-instructores` entregan métricas agregadas (totales, activos, en riesgo, técnicos/transversales). Se consumen desde el dashboard y páginas de gestión.

---

#### RF-26: Versionamiento Lógico de Guías

| Estado | ✅ Cumplido (a nivel de esquema) |
|--------|-----------------------------------|

La entidad `GuiaAprendizaje` incluye el campo `Version` y un `Estado` con valores `Borrador`, `Publicado`, `Cerrado`. Esto permite representar la evolución de una guía a nivel lógico, aunque a nivel de archivo binario se mantiene la regla de RN-REPO-07 ("no hay reemplazo de archivo: eliminar y recargar").

---

#### RF-27: Auto-Migración y Seeder al Arranque

| Estado | ✅ Cumplido |
|--------|-------------|

`Program.cs` aplica migraciones automáticamente al arranque de la API y ejecuta un seeder de catálogos (Rol, TipoDocumento). Este comportamiento es parte del contrato operativo del sistema.

---

#### RF-28: Importación Masiva de Aprendices

| Campo | Detalle |
|-------|---------|
| **ID** | RF-28 |
| **Prioridad** | Alta |
| **Actores** | Administrador, Sistema |
| **Estado** | ✅ Cumplido |
| **Componente** | `ImportadorAprendices` (`AdsoLabs.Infrastructure.Services`) |

**Descripción:**
Permite cargar masivamente aprendices a una ficha existente desde dos formatos de Excel. La ficha destino se especifica por parámetro de ruta (no se infiere del archivo).

**Detección automática de formato (por extensión):**

| Extensión | Origen | Lectura |
|-----------|--------|---------|
| `.xlsx` | Plantilla oficial generada por `PlantillaAprendizGenerator` | Lectura directa fila por fila |
| `.xls` | Reporte de Juicios Evaluativos de SOFIA Plus | Reutiliza `ImportadorReporteJuicios.NormalizarReporteSofia` y deduplica por documento (último estado encontrado gana) |

**Formato de la plantilla oficial (`.xlsx`):**

| Fila | Contenido |
|------|-----------|
| Fila 1 | Encabezados (se omite) |
| Fila 2+ | Una fila por aprendiz |

| Columna | Campo | Obligatorio | Notas |
|---------|-------|-------------|-------|
| A | `TipoDocumento` | Sí | Abreviatura: `CC`, `TI`, `CE`, `PPT`, `PA`, `RC` |
| B | `NumeroDocumento` | Sí | Identificador único |
| C | `Nombres` | Sí | |
| D | `Apellidos` | Sí | |
| E | `Estado` | No | Vacío ⇒ `EN FORMACION`. Valores válidos: `EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO`, `TRASLADADO` (case-insensitive). |

**Procesamiento (idempotente):**
1. Validación global: la `Ficha` indicada por `numeroFicha` debe existir en BD; si no, se aborta con error.
2. Por cada fila válida:
   - Resolver `IdTipoDocumento` desde el catálogo en memoria. Si no existe, registra error de fila y continúa.
   - **Upsert `Persona`** por `NumeroDocumento` (crea si no existe, actualiza nombres/apellidos si existe).
   - **Upsert `AprendizPerfil`** asociado a la persona (crea si la persona no tiene perfil de aprendiz).
   - **Vincular a la ficha** (`FichaAprendiz`) con el estado indicado, aplicando la regla de traslado automático (`IAprendizService.VincularAprendizFichaAsync`).
3. Errores individuales (tipo de documento inválido, campos vacíos, etc.) no detienen el procesamiento del resto.

**Restricciones:**
- ❌ NO crea fichas (la ficha debe existir).
- ❌ NO crea cuentas de `Usuario`; solo `Persona` + `AprendizPerfil`.
- ❌ NO registra trazabilidad en `ImportacionArchivo` (sin requisito de historial aún).
- ❌ NO elimina vínculos existentes (importar el mismo archivo dos veces es inocuo).

**Resultado devuelto (`ResultadoImportacionAprendizDto`):**
- `TotalFilas`, `FilasOk`, `FilasError`, `AprendicesCreados`, `AprendicesActualizados`, `VinculosCreados`, `VinculosActualizados`, `Errores[]`.

**Reglas:** RN-IMPORT-APR-01 a RN-IMPORT-APR-06.

---

#### RF-29: Importación Masiva de Fichas

| Campo | Detalle |
|-------|---------|
| **ID** | RF-29 |
| **Prioridad** | Alta |
| **Actores** | Administrador, Sistema |
| **Estado** | ✅ Cumplido |
| **Componente** | `ImportadorFichas` (`AdsoLabs.Infrastructure.Services`) |

**Descripción:**
Permite cargar masivamente fichas del programa ADSO desde la plantilla oficial Excel (`PlantillaFichasGenerator`). Crea fichas nuevas o actualiza datos básicos de las existentes, y vincula automáticamente el catálogo completo de competencias activas.

**Formato de la plantilla (`.xlsx`):**

| Fila | Contenido |
|------|-----------|
| Fila 1 | Encabezados (se omite) |
| Fila 2+ | Una fila por ficha |

| Columna | Campo | Obligatorio | Notas |
|---------|-------|-------------|-------|
| A | `NumeroFicha` | Sí | Identificador único de la ficha |
| B | `Jornada` | Sí | `Mañana` o `Tarde` (case-insensitive) |
| C | `FechaInicio` | Sí | Formato parseable por `DateTime.TryParse` (ej. `DD/MM/AAAA`) |
| D | `FechaFin` | No | Vacío ⇒ ficha en curso |
| E | `Modalidad` | Sí | `Presencial` o `Virtual` (case-insensitive) |

El campo `Nombre` se completa automáticamente con `"ANALISIS Y DESARROLLO DE SOFTWARE"`; el Excel no debe incluirlo.

**Procesamiento (idempotente):**
1. Carga del catálogo de `Competencia` con `Estado != "Clausurada"` una sola vez (incluye competencias `Practica` e `Induccion`).
2. Por cada fila válida:
   - **Upsert `Ficha`** por `NumeroFicha`:
     - Si no existe → crear con `Nombre = ANALISIS Y DESARROLLO DE SOFTWARE`, `Estado = "Activa"`, `FechaCreacion = UtcNow`.
     - Si existe → actualizar `Jornada`, `Modalidad`, `FechaInicio`, `FechaFin`. `Nombre`, `Estado` y `FechaCreacion` no se tocan.
   - **Auto-vinculación de competencias:** para cada competencia activa del catálogo, crear `FichaCompetencia` (`Estado = "Pendiente"`, `TotalHoras = NULL`, `HorasEjecutadas = 0`) si el vínculo aún no existe.
3. Errores de formato (fechas inválidas, jornada/modalidad no válida, campos vacíos) se registran y la fila se omite, pero las demás continúan.

**Restricciones:**
- ❌ El Excel **no** incluye códigos de competencia: todas las fichas comparten el mismo catálogo.
- ❌ NO elimina vínculos `FichaCompetencia` existentes.
- ❌ NO modifica `FichaCompetencia` ya en estado `Vista` ni `Programada` (solo crea las que falten).
- ❌ NO crea aprendices ni los vincula.
- ❌ NO registra trazabilidad en `ImportacionArchivo`.

**Resultado devuelto (`ResultadoImportacionFichaDto`):**
- `TotalFilas`, `FilasOk`, `FilasError`, `FichasCreadas`, `FichasActualizadas`, `CompetenciasVinculadas`, `FichasAfectadas[]`, `Errores[]`.

**Reglas:** RN-IMPORT-FICHA-01 a RN-IMPORT-FICHA-05.

---

### 3.11 Nuevos Requisitos (v3.3/v3.4) — Módulos detectados por auditoría de código

> Los siguientes requisitos formalizan módulos completos que ya existían en el código pero no figuraban en ninguna versión previa del SRS. Se detectaron en la auditoría Review Mode de julio 2026.

#### RF-30: Monitorías Académicas

| Campo | Detalle |
|-------|---------|
| **ID** | RF-30 |
| **Prioridad** | Media |
| **Actores** | Administrador, Instructor, Aprendiz (monitor) |
| **Estado** | ✅ Cumplido |
| **Componente** | `MonitoriaController`, `MonitoriaAppService`, `MonitoriaQueryService` |

**Descripción:**
Permite designar aprendices como monitores de una competencia/ficha, programar sesiones de monitoría, gestionar inscripciones de otros aprendices, registrar asistencia e informes de cada sesión, y consultar asistencias/informes recibidos por instructor.

**Endpoints (`api/`):** `asignar-monitor` (POST), `desactivar-monitor/{idAprendiz}` (PUT), `obtener-monitores`, `obtener-estado-monitor`, `obtener-monitorias-vigentes`, `obtener-mis-sesiones-monitoria`, `crear-sesion-monitoria` (POST), `cancelar-sesion-monitoria/{id}` (PUT), `obtener-inscritos-monitoria/{id}`, `inscribirse-monitoria` (POST), `desinscribirse-monitoria/{id}` (DELETE), `registrar-asistencia-monitoria` (POST), `registrar-informe-monitoria` (POST), `obtener-asistencias-recibidas-monitoria`, `obtener-informes-recibidos-monitoria`.

**Entidades:** `MonitorPerfil`, `SesionMonitoria`, `InscripcionMonitoria`, `AsistenciaMonitoria`, `InformeMonitoria`, `InformeMonitoriaAprendiz`, `SolicitudAsignacion`.

**UI:** `MonitoriasAdmin.razor` (vista administrativa). **Hallazgo de auditoría:** no se localizó una página Blazor específica para el flujo del aprendiz-monitor (inscribirse, registrar informe); verificar si vive embebida en otra página o si es un pendiente de UI.

**Reglas:** RN-MONITOR-01 a RN-MONITOR-04 (nuevas, ver §6).

---

#### RF-31: Predicción de Compatibilidad y Desempeño con IA

| Campo | Detalle |
|-------|---------|
| **ID** | RF-31 |
| **Prioridad** | Media |
| **Actores** | Administrador, Instructor |
| **Estado** | ✅ Cumplido |
| **Componente** | `PrediccionIAController`, `PrediccionIAService`, `AiAnalysisService` |

**Descripción:**
Genera predicciones de compatibilidad instructor–competencia y de desempeño de aprendizaje de una ficha-competencia, apoyándose en un modelo de lenguaje externo, respetando la fase formativa de la ficha/competencia (ver RF-33, `FaseFormativaHelper`). También gestiona solicitudes de asignación (propuesta de instructor a competencia) con flujo de aprobación, para asignación de monitores/instructores.

**Integración externa:** llama a la **API de Anthropic Claude** (`https://api.anthropic.com/v1/messages`, `Anthropic:ApiKey` en configuración) desde `AiAnalysisService`. Modelo actual: **Claude Haiku** (`claude-haiku-4-5-20251001`), migrado en julio 2026 desde una integración previa con Groq/LLaMA 3.3 (corregido en v3.4). Ver riesgo de privacidad en §8.

**Endpoints (`api/PrediccionIA`, roles `Administrador,Instructor`):** `compatibilidad` (GET), `competencias-asignadas` (GET), `prediccion-aprendizaje` (GET, admite `forzarActualizacion`), `solicitud` (POST), `solicitudes` (GET), `solicitud/{id}/responder` (PUT).

**Reglas:** RN-PREDICCION-01 a RN-PREDICCION-03 (nuevas, ver §6).

---

#### RF-32: Notificaciones en Tiempo Real

| Campo | Detalle |
|-------|---------|
| **ID** | RF-32 |
| **Prioridad** | Baja |
| **Actores** | Todos los roles autenticados |
| **Estado** | ✅ Cumplido |
| **Componente** | `NotificacionController`, `NotificacionHub`, `NotificacionRepository` |

**Descripción:**
Bandeja de notificaciones por usuario con conteo de no leídas, marcado individual y masivo como leídas, y entrega en tiempo real vía SignalR (`NotificacionHub`) además del API REST de consulta.

**Endpoints (`api/notificaciones`):** `GET /` (bandeja), `GET /no-leidas` (conteo), `PUT /{id}/leida`, `PUT /marcar-todas-leidas`.

**Entidad:** `Notificacion`.

**Hallazgo de auditoría:** `NotificacionController` **no tiene atributo `[Authorize]` propio** (a diferencia de `PrediccionIAController` o `DashboardController`); depende únicamente del filtro global `AuthorizeFilter` configurado en `Program.cs` (exige solo estar autenticado, sin restricción de rol). Dado que la bandeja se filtra por `idUsuario` del token, el riesgo es bajo, pero se recomienda revisar si corresponde algún rol explícito.

**Hallazgo adicional (confirmado en `AGENTS.md` v4.2, v3.4):** el Hub de SignalR (`NotificacionHub`) en sí **no pasa por el mismo pipeline de autorización que los controllers REST** — es un pendiente reconocido por el propio equipo, distinto del hallazgo anterior (que es sobre el controller REST, no sobre el Hub).

**Reglas:** RN-NOTIF-01 a RN-NOTIF-02 (nuevas, ver §6).

---

#### RF-33: Fase Formativa de Competencias

| Campo | Detalle |
|-------|---------|
| **ID** | RF-33 |
| **Prioridad** | Media |
| **Actores** | Administrador, Instructor |
| **Estado** | ✅ Cumplido |
| **Componente** | `FaseFormativaHelper` |

> Nuevo en v3.4 — módulo detectado en `AGENTS.md` v4.2, no documentado en ninguna versión previa del SRS.

**Descripción:**
Cada competencia del catálogo tiene asignada una fase formativa: **Inducción, Análisis, Planeación, Ejecución, Evaluación o Transversal**. La fase es visible en Fichas > Competencias, en Asignación de Competencias, y en la pestaña **"Ruta Formativa"** (vista Kanban por fase) de Fichas. El módulo de Predicción IA (RF-31) respeta la fase formativa al generar predicciones de compatibilidad/aprendizaje, mediante `FaseFormativaHelper`.

**Reglas:** pendiente de asignar identificadores RN-FASE-* formales; se documenta la existencia del módulo, no se detalla su regla de negocio completa (fuera del alcance de esta auditoría — revisar directamente con el equipo si se requiere el detalle exacto de transición entre fases).

---

## 4. Historias de Usuario

(Se conservan las historias HU-01 a HU-14 de la v3.0. Las únicas actualizaciones:

- **HU-01:** retirar la mención a "JWT" del criterio de aceptación. La sesión de cara al navegador se gestiona por cookie firmada (existe además un JWT interno servidor-a-servidor Web↔API, ver RN-AUTH-09, que no cambia esta historia).
- **HU-08:** ✅ Cumplida (corregido en v3.4) — la generación de PDF usa jsPDF con **datos reales** vía `InformeAprendizApiService`, ya no mock.
- **HU-14:** ✅ Cumplida (corregido en v3.4) — la UI del repositorio (`Repositorio.razor`, `Guias.razor`) ya está implementada.

El texto íntegro se mantiene sin cambios semánticos en las demás historias.)

---

## 5. Flujos de Usuario Detallados

(Los flujos 1 a 5 descritos en la v3.0 permanecen válidos. **Ajuste general:** donde el flujo mencionaba "JWT" o "token de sesión JWT", debe leerse "cookie de sesión `AdsoLabs.Auth`".)

---

## 6. Reglas de Negocio Consolidadas

### RN-AUTH: Autenticación y Control de Acceso

| ID | Regla | Estado |
|----|-------|--------|
| RN-AUTH-01 | Las contraseñas se hashean usando **PBKDF2-SHA256 con 100.000 iteraciones y salt único por usuario** | ✅ |
| RN-AUTH-02 | En primer login, la contraseña temporal es el número de documento del usuario | ✅ |
| RN-AUTH-03 | El mensaje de error de login no distingue entre usuario inexistente y contraseña incorrecta | ✅ |
| RN-AUTH-04 | Bloqueo: 5 intentos fallidos en 15 min → 30 min de bloqueo | ✅ |
| RN-AUTH-05 | Los administradores NUNCA se bloquean | ✅ |
| RN-AUTH-06 | Instructores y aprendices SÍ se bloquean | ✅ |
| RN-AUTH-07 | Un usuario con `Activo = false` no puede iniciar sesión | ✅ |
| RN-AUTH-08 | `UltimoAcceso` se actualiza en cada login exitoso | ✅ |
| RN-AUTH-09 (v3.3) | La API (`AdsoLabs.API`) exige un JWT Bearer de corta duración para el tráfico servidor-a-servidor desde `AdsoLabs.Web`, emitido internamente a partir de la cookie ya validada del usuario. **No es JWT expuesto al navegador ni sustituye la sesión por cookie**; es una capa interna de confianza Web↔API. Todos los controladores exigen autenticación por defecto (`AuthorizeFilter` global en `Program.cs`); la restricción por rol se aplica selectivamente con `[Authorize(Roles=...)]` por controlador | ✅ (aclaración, no contradice RN-AUTH-01 a 08) |

### RN-ACTIV: Activación de Cuenta

| ID | Regla | Estado |
|----|-------|--------|
| RN-ACTIV-01 | Tokens expiran a los 30 minutos | ✅ |
| RN-ACTIV-02 | Cooldown de 5 minutos entre solicitudes | ✅ |
| RN-ACTIV-03 | Solo un token válido activo por usuario | ✅ |
| RN-ACTIV-04 | Usuario debe existir previamente en el sistema | ✅ |
| RN-ACTIV-05 | Correo único en todo el sistema | ✅ |
| RN-ACTIV-06 | `PasswordResetToken` se usa para activación y recuperación | ✅ |
| RN-ACTIV-07 | Validación de complejidad de contraseña (mín. 8 caracteres, mayúscula, número, carácter especial) | ✅ (`PasswordPolicyValidator`, corregido en v3.4) |

### RN-FICHA: Gestión de Fichas y Competencias

| ID | Regla | Estado |
|----|-------|--------|
| RN-FICHA-01 | Progreso = horas completadas / total horas × 100 | ⚠️ (sin endpoint dedicado) |
| RN-FICHA-02 | Pendiente: ningún resultado programado | ✅ |
| RN-FICHA-03 | Programada: al menos un resultado con instructor + fechas + horas | ✅ |
| RN-FICHA-04 | Vista: todos programados y todos `EN FORMACION` con juicio `APROBADO` | ✅ |
| RN-FICHA-05 | Σ horas de resultados = total horas de la competencia | ✅ |
| RN-FICHA-06 | Competencia `Clausurada` visible pero no asignable | ✅ |

### RN-PROG: Programación

| ID | Regla | Estado |
|----|-------|--------|
| RN-PROG-01 | Programación por resultado individual | ✅ |
| RN-PROG-02 | Un resultado solo puede programarse una vez por ficha-competencia | ✅ |
| RN-PROG-03 | Instructor, Horas > 0, HoraInicio < HoraFin | ✅ |
| RN-PROG-04 | FechaInicio = primer resultado programado | ✅ |
| RN-PROG-05 | FechaFin = último resultado programado | ✅ |
| RN-PROG-06 | Sin conflictos de horario por instructor | ✅ |
| RN-PROG-07 | Sin solapamientos en misma ficha-competencia | ✅ |
| RN-PROG-08 | Instructor existente y activo | ✅ |
| RN-PROG-09 | Ficha en estado que permita programar | ✅ |

### RN-SOFIA: Importación del Reporte de Juicios Evaluativos

| ID | Regla | Estado |
|----|-------|--------|
| RN-SOFIA-01 | Solo `.xls` "Reporte de Juicios Evaluativos" exportado por SOFIA Plus | ✅ |
| RN-SOFIA-02 | La fila de encabezado se localiza dinámicamente (busca "TIPO" + "DOCUMENTO" + "NOMBRE" en las primeras 20 filas; fallback a fila 9 0-based). La cabecera se extrae escaneando las primeras 15 filas por palabras clave. | ✅ |
| RN-SOFIA-03 | Estados de aprendiz válidos en BD: `EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO`, `TRASLADADO`. La normalización es tolerante (cualquier texto con `TRASLAD`, `RETIRO VOLUNTARIO`, `CANCEL` mapea al canónico; el resto se asume `EN FORMACION`). | ✅ |
| RN-SOFIA-04 | Juicios válidos: `APROBADO` y `POR EVALUAR`. Las filas `POR EVALUAR` no generan `JuicioResultado`; solo materializan el FCR y actualizan el estado del aprendiz. | ✅ |
| RN-SOFIA-05 | `Vista` ⇔ todos los FCRs de la `FichaCompetencia` están `Completado` (todos los aprendices `EN FORMACION` con `APROBADO` en cada resultado). | ✅ |
| RN-SOFIA-06 | `En Curso` ⇔ ≥1 FCR `Completado` **o** ≥1 FCR `Programado`, pero no todos `Completado`. | ✅ |
| RN-SOFIA-07 | `Pendiente` ⇔ 0 FCRs `Completado` y 0 FCRs `Programado`. | ✅ |
| RN-SOFIA-08 | **Atomicidad por fila:** cada DTO se procesa en su propia transacción. Un error de fila no detiene la importación; se traza en `ImportacionError`. (No hay rollback global salvo en RN-SOFIA-09.) | ✅ |
| RN-SOFIA-09 | Si la ficha **no existe** en BD, se rechaza el archivo completo (estado `Error`); el importador no crea fichas. | ✅ |
| RN-SOFIA-10 | El importador **no crea aprendices**. Si el documento no existe en `AprendizPerfil`, la fila se rechaza con error registrado y la importación continúa con las demás. | ✅ |
| RN-SOFIA-11 | Las filas con código de competencia `590803` o cuyo nombre contenga `ETAPA PRACTICA` / `ETAPA PRODUCTIVA` se procesan como Etapa Práctica: se omite todo procesamiento académico (competencia, FC, plan, resultado, juicio). | ✅ |
| RN-SOFIA-12 | Un juicio `APROBADO` solo se persiste si el `FichaCompetenciaResultado` ya está `Programado` o `Completado`. Si está `Pendiente`, el juicio se omite silenciosamente (no es error). | ✅ |
| RN-SOFIA-13 | El estado `Vista` de una `FichaCompetencia` **nunca se degrada** por reimportación: una vez alcanzado, no transita de vuelta a `En Curso` ni `Pendiente`. | ✅ |

### RN-IMPORT-APR: Importación Masiva de Aprendices

| ID | Regla | Estado |
|----|-------|--------|
| RN-IMPORT-APR-01 | Detección de formato por extensión: `.xlsx` ⇒ plantilla oficial; `.xls` ⇒ Reporte de Juicios SOFIA Plus reutilizando `NormalizarReporteSofia`. | ✅ |
| RN-IMPORT-APR-02 | La ficha destino se toma del parámetro de ruta (`numeroFicha`); el número de ficha del Excel se ignora. La ficha debe existir; si no, se aborta. | ✅ |
| RN-IMPORT-APR-03 | Importación idempotente: `Persona` y `AprendizPerfil` se hacen upsert; `FichaAprendiz` se crea o actualiza. | ✅ |
| RN-IMPORT-APR-04 | Estado vacío en la plantilla ⇒ `EN FORMACION`. Estado inválido ⇒ error de fila; las demás continúan. | ✅ |
| RN-IMPORT-APR-05 | El importador NO crea `Usuario` (la activación de cuenta es proceso separado). | ✅ |
| RN-IMPORT-APR-06 | Vinculación a la ficha aplica la regla de traslado automático (`IAprendizService.VincularAprendizFichaAsync`). | ✅ |

### RN-IMPORT-FICHA: Importación Masiva de Fichas

| ID | Regla | Estado |
|----|-------|--------|
| RN-IMPORT-FICHA-01 | Solo `.xlsx` con la plantilla oficial. Columnas: `NumeroFicha`, `Jornada` (`Mañana`/`Tarde`), `FechaInicio`, `FechaFin` (opcional), `Modalidad` (`Presencial`/`Virtual`). | ✅ |
| RN-IMPORT-FICHA-02 | El nombre del programa se asigna automáticamente como `"ANALISIS Y DESARROLLO DE SOFTWARE"`; no debe figurar en el Excel. | ✅ |
| RN-IMPORT-FICHA-03 | Upsert por `NumeroFicha`: crear si no existe; actualizar `Jornada`, `Modalidad`, `FechaInicio`, `FechaFin` si existe. `Nombre`, `Estado` y `FechaCreacion` no se sobrescriben en actualización. | ✅ |
| RN-IMPORT-FICHA-04 | Las fichas nuevas se vinculan automáticamente a TODAS las competencias activas (`Estado != "Clausurada"`) del catálogo, en estado `Pendiente`. Vínculos existentes no se duplican. | ✅ |
| RN-IMPORT-FICHA-05 | Errores de formato (fechas, jornada o modalidad inválidas) se registran por fila y no detienen el proceso. | ✅ |

### RN-ASIST: Asistencia

| ID | Regla | Estado |
|----|-------|--------|
| RN-ASIST-01 | Solo aparecen aprendices `EN FORMACION` | ✅ |
| RN-ASIST-02 | Estados: `Presente`, `Justificado`, `Ausente` | ✅ |
| RN-ASIST-03 | Alerta: 3 consecutivas O 20% de horas | ✅ (`AsistenciaRiesgoCalculator`, corregido en v3.4) |
| RN-ASIST-04 | Registros inmutables una vez confirmados | ❌ |
| RN-ASIST-05 | Solo admin puede corregir con auditoría | ❌ |

### RN-REPO: Repositorio de Guías

| ID | Regla | Estado |
|----|-------|--------|
| RN-REPO-01 | Categorías/Tipos documentales: Guías, Plan Concertado, Plan de Mejoramiento (PlanConcertado), Descargas (vista derivada) + ampliaciones RF-20 | ✅ |
| RN-REPO-02 | Cada documento pertenece a un tipo | ✅ |
| RN-REPO-03 | "Descargas" es historial personal del usuario | ✅ (`GET /api/repositorio/mis-descargas`, con UI) |
| RN-REPO-04 | Formatos permitidos: PDF, DOCX | ✅ (parametrizable vía `TipoArchivo`) |
| RN-REPO-05 | Cada guía asociada a exactamente una competencia | ✅ |
| RN-REPO-06 | Cada guía tiene instructor responsable | ✅ |
| RN-REPO-07 | No hay versionamiento de archivo; eliminar y recargar | ✅ |
| RN-REPO-08 | Permisos: Admin CRUD; Instructor CRUD de sus competencias; Aprendiz solo lectura | ✅ (validado en `RepositorioAppService`) |
| RN-REPO-09 | Búsqueda por título, archivo, autor, código competencia | ✅ (`Repositorio.razor` con campo de filtro/búsqueda por tab) |
| RN-REPO-10 | Almacenamiento preferido: SharePoint; alternativa: SQL Server | 🚫 Descartado — almacenamiento definitivo en disco local (`LocalArchivoStorageService`), no es un pendiente técnico |

### RN-PATROC: Patrocinio

| ID | Regla | Estado |
|----|-------|--------|
| RN-PATROC-01 | Un aprendiz solo puede tener una etapa activa | ✅ |
| RN-PATROC-02 | Etapa Lectiva: horario contrario a jornada | ✅ |
| RN-PATROC-03 | Etapa Productiva: 6 meses calendario | ✅ |
| RN-PATROC-04 | Módulo solo lectura para instructores; admin modifica | ✅ |

### RN-DASH: Dashboard

| ID | Regla | Estado |
|----|-------|--------|
| RN-DASH-01 | Dashboard del instructor muestra solo sus fichas | ✅ |
| RN-DASH-02 | Gráficos en Blazor Server con Chart.js/JSInterop (Power BI derogado) | ✅ |
| RN-DASH-03 | Dashboard responsivo | ⚠️ (Bootstrap 5 base) |
| RN-DASH-04 | Aprendiz solo edita teléfono y dirección | ✅ (`Profile.razor`) |
| RN-DASH-05 | Aprendiz solo visualiza competencias de su ficha | ✅ |
| RN-DASH-06 | Estados aprendiz: Activo / Inactivo / Completada | ⚠️ (pendiente de verificación puntual en `Profile.razor`) |

### RN-APRENDIZ

| ID | Regla | Estado |
|----|-------|--------|
| RN-APRENDIZ-01 | Estados: `EN FORMACION`, `CANCELADO`, `TRASLADADO`, `RETIRO VOLUNTARIO` | ✅ |
| RN-APRENDIZ-02 | En Riesgo: supera umbral (ver RN-ASIST-03) | ✅ (corregido en v3.4) |
| RN-APRENDIZ-03 | Cambio de estado registrado en historial | ✅ (`HistorialEstadoAprendiz`) |
| RN-APRENDIZ-04 | Aprendiz ≠ EN FORMACION no aparece en asistencia | ✅ |
| RN-APRENDIZ-05 | Al día / Atrasado por asistencia + avance | ✅ (corregido en v3.4, vía `AsistenciaRiesgoCalculator`) |
| RN-APRENDIZ-06 | No se permite registro individual por UI; importación SOFIA obligatoria | ✅ |

### RN-INSTRUCTOR

| ID | Regla | Estado |
|----|-------|--------|
| RN-INSTRUCTOR-01 | Correo único | ✅ |
| RN-INSTRUCTOR-02 | Contraseña temporal: min 8 + mayúscula + número + especial | ✅ (`PasswordPolicyValidator`, corregido en v3.4) |
| RN-INSTRUCTOR-03 | Forzar cambio en primer login | ✅ |
| RN-INSTRUCTOR-04 | Activo/Inactivo; Inactivo no inicia sesión | ✅ |
| RN-INSTRUCTOR-05 | Filtro por competencia muestra solo asignaciones actuales | ✅ |

### RN-REPORTES

| ID | Regla | Estado |
|----|-------|--------|
| RN-REPORTES-01 | Reportes en Blazor Server, sin Power BI | ✅ |
| RN-REPORTES-02 | Solo Instructor/Administrador | ✅ |
| RN-REPORTES-03 | Instructor ve solo sus fichas (filtrado en `ReportesAppService`/query, no solo UI) | ✅ |
| RN-REPORTES-04 | Admin ve reportes globales | ✅ |
| RN-REPORTES-05 | Exportación PDF y Excel | ✅ (PDF vía jsPDF en `Informe.razor`; Excel server-side en `Reportes.razor`) |
| RN-REPORTES-06 | Contenido del informe (datos personales, asistencia, competencias, planes, patrocinio) | ⚠️ (confirmado: asistencia, progreso de competencias, juicios, patrocinio; contenido de "planes" sin verificar puntualmente) |

### RN-MONITOR (v3.3): Monitorías Académicas

| ID | Regla | Estado |
|----|-------|--------|
| RN-MONITOR-01 | Un aprendiz se asigna como monitor mediante `POST /asignar-monitor`; puede desactivarse (`PUT /desactivar-monitor/{idAprendiz}`) sin eliminar el historial de sesiones | ✅ |
| RN-MONITOR-02 | Las sesiones de monitoría se crean, cancelan e inscriben con validación de existencia (`KeyNotFoundException`) y de reglas de negocio (`InvalidOperationException`) devueltas como `400`/`404` | ✅ |
| RN-MONITOR-03 | Asistencia e informe de cada sesión se registran por separado (`registrar-asistencia-monitoria`, `registrar-informe-monitoria`) | ✅ |
| RN-MONITOR-04 | El instructor consulta asistencias e informes recibidos, opcionalmente filtrado por `idInstructor` | ✅ |

### RN-PREDICCION (v3.3): Predicción de Compatibilidad con IA

| ID | Regla | Estado |
|----|-------|--------|
| RN-PREDICCION-01 | Solo Administrador e Instructor acceden al módulo (`[Authorize(Roles = "Administrador,Instructor")]`) | ✅ |
| RN-PREDICCION-02 | La predicción de aprendizaje admite forzar recálculo (`forzarActualizacion`); de lo contrario se asume cacheable | ✅ |
| RN-PREDICCION-03 | Errores de la API externa (Anthropic) se traducen a `503 Service Unavailable`, no exponen detalle interno del proveedor | ✅ |

### RN-NOTIF (v3.3): Notificaciones

| ID | Regla | Estado |
|----|-------|--------|
| RN-NOTIF-01 | La bandeja y el conteo de no leídas se filtran siempre por el usuario autenticado (`idUsuario` del token, no parámetro de cliente) | ✅ |
| RN-NOTIF-02 | Entrega en tiempo real vía SignalR (`NotificacionHub`) además de la consulta REST | ✅ |

### RN-AUDIT (v3.1): Auditoría y Trazabilidad

| ID | Regla | Estado |
|----|-------|--------|
| RN-AUDIT-01 | Entidad `Auditoria` con campos `IdUsuario`, `Accion`, `TablaAfectada`, `DatosViejos`, `DatosNuevos`, `Fecha`, `DireccionIp` | ✅ |
| RN-AUDIT-02 | Registro transversal automático vía interceptor EF Core | ✅ (`AuditoriaInterceptor`, corregido en v3.4: cubre `AprendizPerfil`, `InstructorPerfil`, `FichaAprendiz` [cambios de estado], `Archivo` y `Usuario.PasswordHash`) |
| RN-AUDIT-03 | Las importaciones SOFIA se auditan en `ImportacionArchivo` + `ImportacionError` | ✅ |
| RN-AUDIT-04 | `UltimoAcceso` del usuario se actualiza en cada login | ✅ |

---

## 7. Requisitos No Funcionales

### RNF-01: Responsividad Web — ⚠️ Parcial

Bootstrap 5 como base; falta validación explícita en 320-1920px y Lighthouse.

### RNF-02: Seguridad y Privacidad — ⚠️ Parcial

**Cifrado en tránsito:** asumido HTTPS/TLS en despliegue.
**Cifrado en reposo:** contraseñas con **PBKDF2-SHA256 + salt** (no BCrypt); TDE a nivel SQL Server pendiente.
**Control de acceso:** RBAC por claims en cookie `AdsoLabs.Auth`; validación en backend en cada endpoint (`[Authorize(Roles=...)]` por controller).
**Cumplimiento legal:** Ley 1581 (Habeas Data). Consentimiento informado: ✅ **implementado** (checkbox obligatorio en `PasswordChange.razor`, validado también en servidor, persiste `Usuario.ConsentimientoFecha`) — corregido en v3.4. **Sigue pendiente** únicamente la revisión legal del texto del aviso de tratamiento de datos (`wwwroot/docs/aviso-tratamiento-datos.html`), que es un borrador. Ver también §8.1 (advertencia sobre envío de datos a Anthropic en Predicción IA).
**Auditoría:** ver RN-AUDIT — ✅ implementada vía `AuditoriaInterceptor` (corregido en v3.4).

### RNF-03: Rendimiento Multiusuario — ⚠️ Sin pruebas formales

No se han ejecutado pruebas de carga (JMeter/k6). Paginación e índices existen a nivel de repositorio, pero sin métricas documentadas.

### RNF-04: Stack Tecnológico — ✅ Cumplido

Stack real: Blazor Server (.NET 9), ASP.NET Core Web API (.NET 9), SQL Server 2019+, **ASP.NET Core Authentication.Cookies**, **PBKDF2-SHA256** (no BCrypt, no JWT). La redacción previa (JWT + BCrypt) queda derogada por esta v3.1.

### RNF-05: Usabilidad — ⚠️ No medido

Sin encuestas SUS ni pruebas formales.

### RNF-06: Disponibilidad — ⚠️ No medido

Sin SLO formal.

### RNF-07: Trazabilidad y Auditoría — ✅ Cumplido

Ver RN-AUDIT. La tabla `Auditoria` existe y el cableado transversal ya está implementado vía `AuditoriaInterceptor` (`SaveChangesInterceptor` de EF Core) — corregido en v3.4.

### RNF-08: Escalabilidad — ⚠️

Blazor Server mantiene estado en servidor (circuitos); la API es stateless. Soporta los volúmenes definidos en el SRS siempre que se escale adecuadamente el host.

---

## 8. Seguridad y Privacidad

(Sin cambios funcionales respecto a la v3.0 salvo en la descripción del cifrado: hashing de contraseñas con PBKDF2, no BCrypt.)

### 8.1 Advertencia de privacidad — Predicción IA (v3.3)

El módulo de Predicción IA (RF-31) envía datos académicos (competencias, resultados, fichas y, potencialmente, identificadores de aprendices/instructores) a la **API externa de Anthropic Claude** para generar predicciones de compatibilidad y desempeño. Esto tiene implicación directa bajo la **Ley 1581 de 2012 (Habeas Data)**:

- **No está definido formalmente** si se requiere consentimiento informado adicional para el tratamiento de datos por un tercero (proveedor de IA), ni si los prompts enviados anonimizan o seudonimizan los datos personales antes de salir del sistema.
- **Pendiente de definición de negocio:** política de retención/tratamiento de datos por parte de Anthropic (¿se usan para entrenamiento? ¿cuánto se retienen?), y si el flujo requiere actualizar el aviso de privacidad de la plataforma.
- **Recomendación:** antes de un despliegue en producción con datos reales, validar con el equipo legal si el envío de datos académicos a un LLM externo requiere cláusula contractual de tratamiento de datos (DPA) con Anthropic, y si corresponde anonimizar los identificadores antes de construir el prompt.

---

## 9. Glosario Técnico

| Término | Definición |
|---------|------------|
| **ADSO** | Análisis y Desarrollo de Software Orientado — programa del SENA |
| **SOFIA Plus** | Sistema de gestión académica oficial del SENA |
| **Ficha de Formación** | Grupo de aprendices que cursan el mismo programa |
| **Competencia** | Conjunto de conocimientos, habilidades y actitudes |
| **Resultado de Aprendizaje** | Componente específico de una competencia |
| **Juicio Evaluativo** | `APROBADO` o `POR EVALUAR` sobre un resultado |
| **Etapa Lectiva / Productiva** | Fases del plan formativo del aprendiz |
| **PBKDF2** | Password-Based Key Derivation Function 2, algoritmo de derivación/hashing de contraseñas |
| **Cookie Auth** | Autenticación basada en cookie firmada por ASP.NET Core |
| **RBAC** | Role-Based Access Control |
| **RLS** | Row-Level Security. Término heredado de la propuesta original con Power BI (derogada en v3.2); el filtrado por rol/instructor se implementa ahora a nivel de query en el backend. |
| **TDE** | Transparent Data Encryption |
| **DTO** | Data Transfer Object |
| **ORM** | Object-Relational Mapping (EF Core) |

---

## 10. Anexos

### A. Diagrama de Contexto del Sistema

```
┌──────────────┐
│   SOFIA      │
│   Plus       │◄────────┐
└──────────────┘         │
                         │ Importación .xls
                         │
                    ┌────▼────────────┐
 ┌─────────────────►│   ADSO LABS      │◄──────────────────┐
 │                  │ (Dashboard/Reportes│                  │
 │                  │  en Blazor+Chart.js)│                 │
 │                  │ (Archivos en disco  │                 │
 │                  │  local, permanente) │                 │
 │                  └──────────────────┘                   │
 │ Acceso Web                                   Acceso Web │
┌▼──────────────┐  ┌──────────────┐  ┌───────────────────▼┐
│ Administrador │  │  Instructor  │  │     Aprendiz       │
└───────────────┘  └──────────────┘  └────────────────────┘
```

> Nota: los bloques "Power BI" y "SharePoint" fueron retirados del diagrama — ambos son decisiones de producto 🚫 descartadas, no integraciones pendientes. Dashboard, Reportes y almacenamiento de archivos son internos a ADSO Labs (Blazor Server + Chart.js; disco local vía `LocalArchivoStorageService`).

### B. Modelo de Datos Simplificado

```
Usuario ── Persona ── AprendizPerfil ── FichaAprendiz ── Ficha ── FichaCompetencia ── FichaCompetenciaResultado
                   │                └─ HistorialEstadoAprendiz                       (con Sesion y Asistencia)
                   │                └─ Acudiente · ObservacionAprendiz
                   └─ InstructorPerfil

Competencia ── ResultadoAprendizaje ── JuicioResultado
            └─ GuiaAprendizaje / InstrumentoEvaluacion / PlaneacionPedagogica / ProyectoFormativo / DesarrolloCurricular

Patrocinio (con NombreEmpresa, ContactoEmpresa) ── AsistenciaPatrocinio
PasswordResetToken
ImportacionArchivo ── ImportacionError
Auditoria
Notificacion
AprendizPerfil ── MonitorPerfil ── SesionMonitoria ── InscripcionMonitoria / AsistenciaMonitoria / InformeMonitoria / InformeMonitoriaAprendiz
SolicitudAsignacion (Predicción IA / Monitorías)
TipoArchivo · TipoDocumento · Rol (catálogos)
```

> Nota de arquitectura: `Core` solo contiene las entidades base de identidad y catálogo (`Persona`, `Usuario`, `Rol`, `TipoDocumento`, `Ficha`, `Competencia`, `FichaCompetencia`, `AprendizPerfil`, `InstructorPerfil`, `PasswordResetToken`). El resto de las entidades listadas arriba vive en `AdsoLabs.Infrastructure/Models` (decisión de diseño, no un incumplimiento de Clean Architecture).

### C. Referencias

- SRS v2.1 — Especificación original
- SRS v3.0 — Actualización técnica (Febrero 2026)
- SRS v3.1 — Alineación con implementación real (Abril 2026)
- Revisión Técnica de Cumplimiento (Abril 2026)
- `CLAUDE.md` v3.3 — Guía operacional del sistema (Mayo 2026)
- Verificación directa de código: `DashboardController.cs`, `Profile.razor`, `Reportes.razor`, `ReportesAppService.cs` (Julio 2026)
- Ley 1581 de 2012 — Protección de datos personales Colombia
- IEEE 830 — Estándar para SRS

---

**Fin del Documento**

*Este documento es confidencial y de uso exclusivo del equipo de desarrollo de ADSO Labs.*
