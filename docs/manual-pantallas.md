# ADSO Labs — Manual de Pantallas

> **Versión:** 1.0
> **Última actualización:** Julio 2026
> **Alcance:** Documentación funcional de cada pantalla del frontend (`AdsoLabs.Web`, Blazor Server). Dirigido tanto a usuarios finales (Administrador, Instructor, Aprendiz) como a desarrolladores que necesiten entender el comportamiento esperado de cada componente `.razor`.
> **Fuente de verdad:** Este documento describe el comportamiento observado en el código real de `AdsoLabs.Web/Components/Pages/` y sus servicios/controllers asociados. Para reglas de negocio transversales y arquitectura, ver [`AGENTS.md`](../AGENTS.md). Para roles y permisos en detalle, ver [`roles-permisos.md`](roles-permisos.md).

---

## Índice

1. [Autenticación y Acceso de Cuenta](#1-autenticación-y-acceso-de-cuenta)
   - 1.1 [Login](#11-login-login)
   - 1.2 [Acceso (selector activar/recuperar)](#12-acceso-acceso)
   - 1.3 [Activar Cuenta](#13-activar-cuenta-activarcuenta)
   - 1.4 [Recuperar Contraseña](#14-recuperar-contraseña-password)
   - 1.5 [Cambiar Contraseña](#15-cambiar-contraseña-passwordchange)
   - 1.6 [Registro de Instructores](#16-registro-de-instructores-registroinstructores)
2. [Perfiles de Usuario](#2-perfiles-de-usuario)
   - 2.1 [Perfil del Aprendiz](#21-perfil-del-aprendiz-profile)
   - 2.2 [Perfil del Instructor](#22-perfil-del-instructor-perfilinstructor)
3. [Fichas y Competencias](#3-fichas-y-competencias)
   - 3.1 [Fichas (detalle de ficha)](#31-fichas-fichas)
   - 3.2 [Catálogo de Competencias](#32-catálogo-de-competencias-competencia)
   - 3.3 [Asignación de Competencias](#33-asignación-de-competencias-asignacioncompetencias)
   - 3.4 [Gestión de Aprendices](#34-gestión-de-aprendices-aprendiz)
   - 3.5 [Gestión de Instructores](#35-gestión-de-instructores-instructor)
   - 3.6 [Importaciones](#36-importaciones-importaciones)
4. [Asistencia y Patrocinio](#4-asistencia-y-patrocinio)
   - 4.1 [Gestión de Asistencias (programación)](#41-gestión-de-asistencias-asistencias)
   - 4.2 [Gestión de Sesiones](#42-gestión-de-sesiones-asistenciassesiones)
   - 4.3 [Registro de Asistencia](#43-registro-de-asistencia-asistenciasregistro)
   - 4.4 [Patrocinados (seguimiento)](#44-patrocinados-asistenciaspatrocinado)
   - 4.5 [Patrocinios (CRUD)](#45-patrocinios-patrocinios)
5. [Dashboard, Reportes e Informes](#5-dashboard-reportes-e-informes)
   - 5.1 [Inicio / Dashboard](#51-inicio--dashboard-home)
   - 5.2 [Reportes](#52-reportes-reportes)
   - 5.3 [Informe Individual del Aprendiz](#53-informe-individual-del-aprendiz-informe)
   - 5.4 [Predicción IA](#54-predicción-ia-prediccionia)
6. [Repositorio Documental y Monitorías](#6-repositorio-documental-y-monitorías)
   - 6.1 [Repositorio](#61-repositorio-repositorio)
   - 6.2 [Guías (pantalla legacy)](#62-guías-guias--legacy)
   - 6.3 [Monitorías (panel administrativo)](#63-monitorías-monitoriasadmin)

---

## 1. Autenticación y Acceso de Cuenta

### 1.1 Login (`Login.razor`)

**Ruta:** `/` (página raíz) · **Acceso:** público (`[AllowAnonymous]`)

**Propósito:** Punto de entrada único al sistema para los tres roles.

**Campos:** Correo (email), Contraseña (password). Enlace "¿Olvidaste la contraseña?" → `/acceso`. Banner informativo si viene de `?activado=true` (cuenta recién activada).

**Validaciones:**
- Client-side: ningún campo puede quedar vacío ("No pueden haber campos vacios").
- Server-side (`AuthService.ValidarCredencialesAsync`): mensaje genérico "Usuario o contraseña incorrectos" (no distingue usuario inexistente de contraseña errónea, por seguridad). Bloqueo de 5 intentos fallidos en 15 min → 30 min de bloqueo (Administrador nunca se bloquea, ver [`roles-permisos.md`](roles-permisos.md)). Primer login compara contra el número de documento en vez del hash.

**Flujo:**
1. Si el usuario ya tiene sesión activa, se le redirige a `/logouthandler` (fuerza cierre de sesión antes de mostrar el login).
2. Envía credenciales a `POST api/login`.
3. Si es exitoso, se arma un formulario oculto con los datos de sesión y se envía por POST a `/loginhandler`, que crea la cookie de autenticación (`AdsoLabs.Auth`) y redirige según el caso:
   - `PrimerLogin = true` → `/passwordchange` (obligatorio).
   - Administrador / Instructor → `/home`.
   - Aprendiz (login normal) → `/perfil`.

**Relación con otros módulos:** primer nodo del flujo de autenticación completo; enlaza con Acceso, ActivarCuenta, Password y PasswordChange.

---

### 1.2 Acceso (`Acceso.razor`)

**Ruta:** `/acceso` · **Acceso:** público

**Propósito:** Bifurcación entre "Activar cuenta" (primer ingreso, aprendiz) y "Recuperar contraseña" (usuario existente que olvidó su clave).

**Componentes:** Dos tarjetas seleccionables (Activar cuenta / Recuperar contraseña) y botón "Continuar" (deshabilitado hasta seleccionar una opción).

**Flujo:** selección de tarjeta → clic en "Continuar" → navega a `/activar-cuenta` o `/password` según la opción elegida. Si el usuario ya está autenticado, se le redirige automáticamente a `/logouthandler`.

---

### 1.3 Activar Cuenta (`ActivarCuenta.razor`)

**Ruta:** `/activar-cuenta` · **Acceso:** público

**Propósito:** Solicitar el enlace de activación para aprendices que ingresan por primera vez (o instructores dados de alta por el administrador).

**Campos:** Número de documento, Correo electrónico.

**Validaciones y restricciones (server-side, `ActivacionCuentaAppService`):**
- Documento debe existir como aprendiz/instructor sin usuario asignado → si no, "No se encontró un aprendiz con ese número de documento."
- Cuenta ya activada → "Esta cuenta ya fue activada. Usa 'Olvidé mi contraseña' si necesitas acceso."
- Correo ya registrado por otro usuario → "Ese correo ya está registrado en el sistema."
- **Cooldown de 5 minutos** entre solicitudes → "Ya se envió un enlace recientemente. Espera 5 minutos antes de intentar de nuevo."
- Persona inactiva → "No es posible activar esta cuenta. El aprendiz se encuentra inactivo en el sistema."
- Token: GUID hasheado con SHA256, **expira a los 30 minutos**, solo uno activo a la vez.

**Flujo:** solicitud → correo con enlace `/activacion-handler?token=...` → el handler valida el token, activa la cuenta y redirige a `/?activado=true`, donde el Login muestra el banner indicando que debe iniciar sesión con su número de documento como contraseña temporal.

---

### 1.4 Recuperar Contraseña (`Password.razor`)

**Ruta:** `/password` · **Acceso:** público

**Propósito:** Solicitar el enlace de recuperación para un usuario que ya cambió su contraseña alguna vez (no aplica a primer login).

**Campos:** Correo electrónico.

**Validaciones (server-side, `RecuperacionPasswordAppService`):**
- Si el usuario está en `PrimerLogin = true`: "Tu cuenta es nueva, debes iniciar sesión y cambiar tu contraseña desde ahí." (no se emite token).
- Cooldown entre solicitudes → "Ya enviamos un correo recientemente. Espera unos minutos."
- Respuesta genérica por seguridad si el correo no existe: "Si el correo institucional está registrado, recibirás un enlace de recuperación."

**Flujo:** solicitud → correo con enlace `/recuperacion-handler?token=...` → el handler valida el token, crea una sesión temporal (claims `CambioPassword=true`, `TokenRecuperacion`) y redirige a `/passwordchange`.

---

### 1.5 Cambiar Contraseña (`PasswordChange.razor`)

**Ruta:** `/passwordchange` · **Acceso:** requiere `PrimerLogin=true` o sesión temporal de recuperación; si ninguna condición se cumple, redirige a `/`.

**Propósito dual:** establecer contraseña en el primer login, o completar una recuperación con token válido. El título y las validaciones cambian según el escenario.

**Campos:** Nueva contraseña, Confirmar contraseña. **Checkbox de consentimiento informado (Ley 1581)** — obligatorio **solo en primer login** — con enlace al aviso de tratamiento de datos (`wwwroot/docs/aviso-tratamiento-datos.html`, borrador pendiente de revisión legal).

**Validaciones:**
- Client-side: campos no vacíos, contraseñas coincidentes, y en primer login, checkbox marcado.
- Server-side (`PasswordPolicyValidator`, mínimo 8 caracteres, mayúscula, número, carácter especial): aplica tanto a `CambiarPasswordPrimerLogin` como a `CambiarPassword`.
- Primer login sin consentimiento marcado → 400 "Debes aceptar el Aviso de Tratamiento de Datos Personales para continuar." (persiste `Usuario.ConsentimientoFecha` al aceptar).

**Flujo:** validación exitosa → mensaje de éxito ("Contraseña establecida correctamente." / "Contraseña actualizada correctamente.") → espera 3 segundos → redirige a `/` para iniciar sesión con la nueva contraseña.

---

### 1.6 Registro de Instructores (`RegistroInstructores.razor`)

**Ruta:** `/registro-instructor` · **Acceso:** solo **Administrador**

**Propósito:** Alta manual de nuevos instructores en el sistema (dato personal + datos de vinculación).

**Campos:** Tipo/Número de documento, Nombre, Apellido, Correo, Teléfono, Fecha de nacimiento, Dirección, Municipio (datos personales); Número de contrato, Especialidad, Tipo de vinculación (Planta/Contratista/Hora Cátedra), Fecha de vinculación, Estado (Activo por defecto).

**Validaciones:** tipo/número de documento, nombre, apellido y correo son obligatorios (correo debe contener `@`); servidor rechaza documentos duplicados.

**Flujo:** al guardar exitosamente se genera el `Usuario` con contraseña temporal = número de documento (mismo mecanismo de primer login que los aprendices) y se muestra un modal de confirmación que redirige a `/instructor`.

---

## 2. Perfiles de Usuario

### 2.1 Perfil del Aprendiz (`Profile.razor`)

**Ruta:** `/perfil` · **Acceso:** solo **Aprendiz**

**Propósito:** Panel central de autogestión del aprendiz: datos personales, acudiente, cronograma, formación, observaciones y — si el aprendiz tiene perfil de **monitor** — gestión de sus propias sesiones de monitoría.

**Estructura por pestañas:**

| Pestaña | Contenido y acciones |
|---|---|
| **Mis Datos** | Documento (solo lectura); edición de fecha de nacimiento, estrato, tipo de población, condición especial, teléfono, municipio, dirección y contacto de emergencia. Cada bloque tiene su propio botón Editar/Guardar/Cancelar. |
| **Acudiente** | Registro o edición de acudiente (Nombre, Parentesco, Teléfono son obligatorios; Correo y Dirección opcionales). Estado vacío ofrece "Registrar acudiente". |
| **Cronograma** | Vista de sesiones programadas en 4 modos: Calendario semanal, Lista, Próximas, Historial (máx. 20). Si el aprendiz está patrocinado, se muestra la tarjeta de patrocinio (empresa, etapa, horario, fechas) en la parte superior. |
| **Mi Formación** | Anillo de progreso general (% de competencias completadas), filtros (Todas/Aprobadas/En Curso/Pendientes) y detalle expandible por ficha → competencia → resultados con juicio. |
| **Observaciones** | Listado de observaciones registradas por instructores/administrador (solo lectura para el aprendiz), ordenadas por fecha descendente. |
| **Monitorías** | Sesiones de monitoría vigentes: inscribirse/desinscribirse (si el aprendiz no es el monitor de esa sesión). |
| **Gestión de Monitorías** *(solo si el aprendiz tiene `MonitorPerfil` activo)* | Crear sesiones de monitoría propias (nombre, jornada, modalidad, horario, fechas), ver inscritos, pasar asistencia a los inscritos y enviar informe de la sesión a un instructor destinatario. |

**Validaciones:** campos de acudiente obligatorios (Nombre, Parentesco, Teléfono); en creación de sesión de monitoría, fecha fin ≥ fecha inicio y horarios válidos.

**Relación con otros módulos:** las asistencias/informes de monitoría enviados desde aquí llegan al panel `MonitoriasAdmin.razor` del instructor destinatario.

---

### 2.2 Perfil del Instructor (`PerfilInstructor.razor`)

**Ruta:** `/perfil-instructor/{IdInstructor}` · **Acceso:** Administrador, Instructor (edición de contacto solo para Admin o el propio instructor)

**Propósito:** Vista de consulta del instructor: datos de contacto, fichas asociadas, competencias asignadas, horario activo y avisos.

**Componentes:**
- Tarjeta de perfil con datos de contacto editables (correo, teléfono) si el usuario es Admin o el mismo instructor.
- Tab **Fichas Asociadas**: lista de fichas donde participa, con progreso y estado (Activa/Finalizada).
- Tab **Competencias asignadas**: filtro por ficha ("Todas las fichas" o una específica), con barra de progreso por horas ejecutadas/totales.
- Tab **Horario**: programaciones activas (resultado, ficha, fechas, horario, estado).
- Panel lateral de **Avisos Importantes** (notificaciones específicas del instructor).

**Relación con otros módulos:** accesible desde `Instructor.razor` (clic en nombre) y desde `Fichas.razor`/tablas que muestren instructor asignado.

---

## 3. Fichas y Competencias

### 3.1 Fichas (`Fichas.razor`)

**Ruta:** `/fichas/{numeroFicha}` · **Acceso:** Administrador, Instructor

**Propósito:** Pantalla central de una ficha de formación: aprendices, competencias, cronograma visual (Kanban), ruta formativa por fase, repositorio documental de la ficha e importación de juicios evaluativos SOFIA Plus.

**Encabezado:** número de ficha, duración en meses, competencias vistas/total, total de aprendices, horas vistas/total, progreso general (%), fase actual, etapa formativa y fechas.

**Secciones (navegación por botones):**

1. **Aprendices** — búsqueda por nombre/documento, paginación (7/página), tabla con estado "Al día"/"Atrasado" e indicador de patrocinio. Clic en fila → `/aprendices/{id}/informe`.
2. **Competencias** — filtros por estado (Todas/Vistas/Pendientes/Programadas/En Curso), búsqueda, paginación. Cada fila es expandible para ver sus resultados de aprendizaje (instructor, fechas, horas, estado, juicios aprobados/total). Botones: "Cargar Juicios Evaluativos" (todos) y "Programar competencia" (solo Administrador, navega a Asignación de Competencias).
3. **Cronograma** — tablero Kanban de 4 columnas por estado (Pendiente/Programada/En Curso/Vista), una tarjeta por competencia con progreso de horas.
4. **Ruta Formativa** — tablero Kanban de 6 columnas por fase formativa (Inducción, Análisis, Planeación, Ejecución, Evaluación, Transversal), vista simplificada sin detalle de resultados.
5. **Repositorio** — sub-pestañas Guías / Planeaciones / Proyecto Formativo, con carga (PDF/DOCX), descarga y eliminación (Admin o instructor propietario del documento).

**Importación de Juicios Evaluativos SOFIA Plus (modal):**
- Acepta únicamente `.xls` (rechaza cualquier otra extensión con mensaje explícito).
- Aviso explícito: "Solo se actualizan estados de aprendices y juicios ya registrados; no se crean fichas ni aprendices nuevos" (ver §7.5 de `AGENTS.md`).
- Resultado mostrado: filas totales, procesadas, con error, aprendices actualizados, juicios insertados/actualizados, y detalle expandible de errores por fila.

**Validaciones:** archivo de juicios debe ser `.xls`; documentos del repositorio deben ser PDF/DOCX; si la ficha no existe, redirige a `/home`.

**Relación con otros módulos:** enlaza con Aprendiz (informe individual), Asignación de Competencias (programación), Repositorio (documentos por ficha) e Importaciones (mismo motor de importación SOFIA).

---

### 3.2 Catálogo de Competencias (`Competencia.razor`)

**Ruta:** `/competencias` · **Acceso:** Administrador, Instructor

**Propósito:** Consulta del catálogo general de competencias: instructores asociados y fichas donde está vigente o clausurada.

**Uso:** selector de competencia + botón "Buscar Competencia" → muestra contadores (Total/Activas/Clausuradas), lista de instructores asociados (seleccionable para filtrar) y tabla de fichas asociadas (ficha, modalidad, estado).

**Es una pantalla de solo lectura** — no permite crear ni editar competencias del catálogo.

---

### 3.3 Asignación de Competencias (`AsignacionCompetencias.razor`)

**Ruta:** `/asignacion-competencias` · **Acceso:** solo **Administrador**

**Propósito:** Programar una competencia dentro de una ficha: configurar la ventana general (fechas + horas totales) y programar cada resultado de aprendizaje individualmente (instructor, horas, horario, fechas), conforme a la regla de negocio de que la programación es por resultado y no por competencia completa (§7.4 `AGENTS.md`).

**Modo lectura (competencia en estado "Vista"):** tarjeta de solo consulta con el detalle de resultados ya completados.

**Modo edición (Pendiente/Programada):**
1. **Datos de la competencia** — tipo (Transversal/Técnica/Clave), selector de competencia (excluye clausuradas, inducción y etapa práctica).
2. **Configuración de la competencia** — fecha inicio, fecha fin, total de horas. Botón "Guardar configuración" crea/actualiza el registro `FichaCompetencia`.
3. **Resultados de Aprendizaje** — por cada resultado: si está "Completado" se muestra de solo lectura; si está "Programado" se puede editar o eliminar la programación; si está "Pendiente" se abre el formulario (instructor, horas, hora inicio/fin, fecha inicio — la fecha fin se autocalcula según horas y horario diario, editable manualmente).
4. **Desglose de horas** — total, consumidas y restantes de la competencia.

**Validaciones:** instructor obligatorio, horas > 0, fechas y horas de inicio/fin obligatorias; la suma de horas de resultados debe coincidir con el total de horas de la competencia (regla §7.3 `AGENTS.md`).

**Relación con otros módulos:** se accede desde `Fichas.razor` (botón "Programar competencia", con preselección de competencia).

---

### 3.4 Gestión de Aprendices (`Aprendiz.razor`)

**Ruta:** `/aprendiz` · **Acceso:** solo **Administrador**

**Propósito:** Listado y gestión de estado de todos los aprendices del programa. No permite alta individual — todo aprendiz se crea vía importación SOFIA Plus (§7.9 `AGENTS.md`).

**Filtros:** ficha, número de documento, búsqueda por nombre. Contadores: total, activos, en riesgo.

**Cambio de estado:** menú desplegable por fila con los estados asignables `EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO` (actualización optimista con reversión si el servidor rechaza el cambio). `TRASLADADO` es de solo lectura — es un estado derivado automáticamente por el sistema, no asignable manualmente desde aquí.

**Relación con otros módulos:** clic en fila (si el aprendiz no está en estado terminal) → `/aprendices/{id}/informe`; los datos provienen de `Importaciones.razor`.

---

### 3.5 Gestión de Instructores (`Instructor.razor`)

**Ruta:** `/instructor` · **Acceso:** Administrador, Instructor (alta y activación/desactivación restringidas a Administrador en la UI)

**Propósito:** Directorio de instructores con filtro por competencia y búsqueda, toggle de estado Activo/Inactivo, y modal de competencias asignadas por instructor.

**Componentes:** contadores (total, activos, técnicos), tabla paginada (6/página) con nombre, especialidad, tipo de vinculación y switch de estado. Botón "Agregar Instructores" (solo Administrador) → `/registro-instructor`.

**Relación con otros módulos:** clic en nombre → `/perfil-instructor/{id}`; el filtro por competencia comparte catálogo con `Competencia.razor` y `AsignacionCompetencias.razor`.

---

### 3.6 Importaciones (`Importaciones.razor`)

**Ruta:** `/importaciones` · **Acceso:** solo **Administrador**

**Propósito:** Centro único de las tres importaciones masivas del sistema.

**Panel 1 — Importar Fichas (RF-29):** acepta únicamente `.xlsx` de la plantilla oficial. Al crear una ficha se auto-vinculan todas las competencias no clausuradas del catálogo en estado Pendiente; al actualizar una existente solo se tocan Jornada/Modalidad/fechas. Botón "Ver plantilla" muestra la estructura de columnas esperada; botón "Descargar plantilla" genera el archivo vacío.

**Panel 2 — Importar Aprendices (RF-28):** requiere seleccionar primero la ficha destino (el número de ficha del archivo, si lo trae, se ignora); acepta `.xlsx` (plantilla propia) o `.xls` (Reporte SOFIA Plus, reutilizando la misma normalización que la importación de juicios).

**Panel 3 — Historial de importaciones de juicios:** tabla paginada (8/página) de las importaciones SOFIA Plus (RF-11) con estado (Procesado/Error/Pendiente), filas OK/con error, y detalle expandible de errores por fila al hacer clic.

**Validaciones:** extensión de archivo estrictamente comprobada antes de enviar (mensajes específicos por panel); tamaño máximo de 20 MB.

**Relación con otros módulos:** alimenta directamente `Fichas.razor` (listado) y `Aprendiz.razor` (listado); comparte el motor de importación de juicios con el modal de `Fichas.razor`.

---

## 4. Asistencia y Patrocinio

### 4.1 Gestión de Asistencias (`Asistencias.razor`)

**Ruta:** `/asistencias` · **Acceso:** Administrador, Instructor (instructor ve solo sus fichas asignadas; Administrador ve todas)

**Propósito:** Vista de la programación académica de una ficha en formato calendario semanal o lista, punto de entrada a la gestión de sesiones.

**Componentes:** selector de ficha (tarjetas), toggle Semanal/Lista, navegación de semana (anterior/siguiente/"Hoy"/"Primera clase"/salto a fecha), calendario de 6 columnas (lunes–sábado, 06:00–22:00) con bloques de competencia coloreados por hash y organizados en carriles quando hay solapamientos.

**Validaciones:** el salto a fecha solo acepta años 2000–2099; si es inválida se ignora.

**Flujo:** clic en un bloque o fila → navega a `/asistencias/sesiones/{idFichaCompetenciaResultado}` conservando el parámetro `?ficha=` para poder volver.

---

### 4.2 Gestión de Sesiones (`AsistenciasSesiones.razor`)

**Ruta:** `/asistencias/sesiones/{IdFichaCompetenciaResultado}` · **Acceso:** Administrador, Instructor

**Propósito:** Crear y consultar las sesiones reales (`Sesion`) de un resultado de aprendizaje programado, y acceder al registro de asistencia de cada una.

**Formulario de nueva sesión:** calendario con navegación por mes; celdas bloqueadas para domingo y festivos colombianos (validado con `FestivosColombiaHelper`); sábado permitido pero exige checkbox de confirmación explícita; horario de inicio/fin.

**Tabla de sesiones:** fecha, horario, estado (Abierta/Finalizada/Cancelada), barra de asistencia (presentes+tarde vs. ausentes+justificados) y botón Registrar/Editar (deshabilitado si la sesión está Cancelada).

**Validaciones:** fecha obligatoria y dentro del rango del resultado; domingo y festivo bloquean la creación; sábado requiere el checkbox de confirmación marcado.

**Flujo:** al crear la sesión, navega directamente al registro de asistencia de esa sesión.

---

### 4.3 Registro de Asistencia (`AsistenciasRegistro.razor`)

**Ruta:** `/asistencias/registro/{IdFichaCompetenciaResultado}/{IdSesion}` · **Acceso:** Administrador, Instructor

**Propósito:** Capturar el estado de asistencia (Presente, Tarde, Ausente, Justificado) de cada aprendiz **en formación** para una sesión concreta.

**Componentes:** resumen de conteos en tiempo real por estado, botones de marcado masivo ("Marcar todos: Presente" / "Marcar todos: Ausente"), tabla con selector de estado por aprendiz y campo de motivo (solo visible/habilitado si el estado es Justificado, se limpia automáticamente al cambiar a otro estado).

**Restricciones:** si la sesión está Cancelada, no se permite registrar (mensaje explícito); si no hay aprendices en formación, se informa igualmente.

**Flujo:** al guardar, la sesión pasa a estado Finalizada y el botón de guardado queda deshabilitado.

> ⚠️ Nota de negocio (§7.6 `AGENTS.md`): la inmutabilidad de registros confirmados **no está garantizada aún en código** — cualquier usuario con acceso puede reabrir/editar sin control adicional. Pendiente de implementar.

---

### 4.4 Patrocinados (`AsistenciasPatrocinado.razor`)

**Ruta:** `/asistencias-patrocinados` · **Acceso:** Administrador (lectura/escritura), Instructor (lectura, puede asignar horario)

**Propósito:** Seguimiento de asistencia académica y asignación de horario de los aprendices en etapa de patrocinio.

**Componentes:** filtros por ficha, etapa (Lectiva/Productiva) y búsqueda (aprendiz/empresa); KPIs (total patrocinados, en riesgo &lt;70%, sin horario asignado); grid de tarjetas con barra de asistencia y estado de horario; modal de detalle con resumen de asistencia académica y formulario de asignación/eliminación de horario de patrocinio.

**Validación de negocio:** el aviso del formulario recuerda que, para etapa Lectiva, el horario de patrocinio debe ser en jornada contraria a la de formación (§7.7 `AGENTS.md`).

---

### 4.5 Patrocinios (`Patrocinios.razor`)

**Ruta:** `/patrocinios` · **Acceso:** Administrador (CRUD completo), Instructor (solo lectura — sin botón de creación/edición/desactivación en la UI, conforme a §7.7 `AGENTS.md`)

**Propósito:** Ciclo de vida completo de los patrocinios (creación, edición, desactivación).

**Filtros:** ficha, etapa, estado (Activos/Inactivos/Todos), búsqueda por aprendiz/empresa.

**Modal Crear/Editar:** en creación se elige ficha (carga dinámicamente los aprendices en formación de esa ficha) y aprendiz; en edición esos dos campos no se muestran (el aprendiz ya está fijado). Campos comunes: Etapa, Empresa, Contacto, Fecha inicio/fin.

**Desactivación:** modal de confirmación explícito ("se puede revertir desde la edición").

**Validación:** aprendiz obligatorio al crear.

---

## 5. Dashboard, Reportes e Informes

### 5.1 Inicio / Dashboard (`Home.razor`)

**Ruta:** `/home` · **Acceso:** Administrador, Instructor

**Propósito:** Resumen visual de actividad tras el login: fichas asignadas (o todas, si Administrador), progreso general, estado de competencias y alertas. Alimentado por `/api/mi-dashboard` y derivados.

**Componentes:** tarjetas KPI (perfil, progreso general, fichas activas, horas, instructores técnicos), gráfico donut (Chart.js) de estado de competencias, gráficos de barras de progreso por ficha, tarjeta de alertas (baja asistencia &lt;70%, retraso de horas, fichas al día, competencias que cierran esta semana), y barras de estado general de fichas (Crítico/Riesgo/Excelencia).

> ⚠️ Nota de negocio (§7.8 `AGENTS.md`): los umbrales exactos de Crítico/Riesgo/Excelencia y el criterio de "Próx. a cerrar" **no están formalizados** como constantes de negocio — pendiente de definición.

---

### 5.2 Reportes (`Reportes.razor`)

**Ruta:** `/reportes` · **Acceso:** Administrador, Instructor (instructor filtrado a sus fichas / su propia carga si es "Por Instructor")

**Propósito:** Generador de los 8 tipos de reporte académico, cada uno con exportación a Excel server-side (bytes descargados vía `HttpClient` autenticado + JS interop, **nunca** por enlace directo, porque el navegador no porta el JWT interno).

**Los 8 reportes:**

1. **Informe Individual** — selector Ficha → Aprendiz, enlaza al PDF de `Informe.razor`.
2. **Asistencia por Ficha** — KPIs de asistencia, donut normal/riesgo, ranking de ausencias, tabla por aprendiz.
3. **Progreso de Competencias** — completadas/en ejecución/pendientes, horas ejecutadas/planeadas por competencia.
4. **Juicios Evaluativos** — acordeón jerárquico Aprendiz → Competencia → Resultado con juicio (Aprobado/Por Evaluar).
5. **Resumen Global** — vista consolidada de todas las fichas (asistencia y progreso promedio).
6. **Por Instructor** — carga académica: fichas, competencias, horas ejecutadas/planeadas; instructor ve automáticamente el suyo, Administrador puede elegir cualquiera.
7. **Por Competencia** — estado de una competencia específica en cada ficha donde está asignada.
8. **Patrocinio** — aprendices patrocinados, etapa, empresa y asistencia.

**Validaciones:** el servidor responde `403 Forbid` si el usuario (instructor) intenta consultar una ficha fuera de su alcance.

---

### 5.3 Informe Individual del Aprendiz (`Informe.razor`)

**Ruta:** `/aprendices/{IdAprendiz}/informe` · **Acceso:** Administrador, Instructor

**Propósito:** Informe académico completo de un aprendiz, con exportación a **PDF 100% client-side** (`html2canvas` + `jsPDF` sobre una plantilla HTML oculta — no depende del servidor).

**Pestañas:** Datos Personales (con avisos automáticos de resultados pendientes de calificar), Acudiente (lectura/edición con validación de Nombre/Parentesco/Teléfono obligatorios) y Observaciones (textarea de hasta 500 caracteres + historial).

**Restricción de navegación:** si el estado del aprendiz es Cancelado, Retiro Voluntario o Trasladado, redirige de vuelta a `/aprendiz`.

**Relación con otros módulos:** se accede desde `Reportes.razor` (Informe Individual) y desde el listado de aprendices en `Fichas.razor`/`Aprendiz.razor`.

---

### 5.4 Predicción IA (`PrediccionIA.razor`)

**Ruta:** `/prediccion-ia` · **Acceso:** Administrador, Instructor

**Propósito:** Asistente de asignación de competencias y predicción de aprendizaje basado en **Anthropic Claude Haiku**, respetando la fase formativa de la ficha/competencia (`FaseFormativaHelper`).

**Tres pestañas:**

1. **Asignación de competencias inteligente** — la IA compara el historial del instructor contra competencias pendientes de fichas activas (sugeribles solo si su fase ≤ fase actual de la ficha + 1, salvo Transversal que siempre es sugerible) y devuelve un porcentaje de compatibilidad, fortalezas y sugerencias razonadas. El instructor puede "Solicitar" una competencia sugerida, generando una `SolicitudAsignacion` en estado Pendiente.
2. **Predicción de aprendizaje** — para una competencia ya asignada al instructor, la IA devuelve tendencia general, temáticas emergentes, ruta de aprendizaje sugerida y KPIs. Resultado cacheado 60 minutos por usuario+competencia; botón "Actualizar" fuerza recálculo.
3. **Solicitudes** *(solo Administrador)* — bandeja de solicitudes de asignación pendientes, con acciones Aprobar/Rechazar.

**Nota técnica:** la predicción de aprendizaje (pestaña 2) **no** aplica todavía el filtro de fase formativa que sí aplica la pestaña 1 — inconsistencia pendiente de resolver, no reportada previamente en `AGENTS.md`.

---

## 6. Repositorio Documental y Monitorías

### 6.1 Repositorio (`Repositorio.razor`)

**Ruta:** `/repositorio` · **Acceso:** Administrador (CRUD completo), Instructor (carga y ve todos los documentos; solo puede cambiar estado o eliminar los que él mismo subió)

**Propósito:** Gestión centralizada de las 6 categorías documentales: Guías de Aprendizaje, Instrumentos de Evaluación, Planeaciones Pedagógicas, Proyectos Formativos, Desarrollo Curricular y Planes Concertados — más una pestaña de "Mis Descargas" (historial personal).

**Componentes:** búsqueda en tiempo real, 7 pestañas con badge de conteo, tabla de documentos (nombre, estado —Publicado/Borrador/Cerrado—, instructor, entidad asociada, fecha, acciones), modal de subida (campos variables según tipo de documento — ver detalle abajo) y modales de cambio de estado / eliminación con confirmación.

**Campos de subida por tipo:**

| Tipo | Título/Nombre | Entidad relacionada | Formatos |
|---|---|---|---|
| Guía | Sí | Ficha → Competencia | PDF, DOCX |
| Instrumento | Sí | Competencia | PDF, DOCX |
| Planeación | No | Ficha → Competencia | PDF, DOCX |
| Proyecto Formativo | Sí | Ficha | PDF, DOCX |
| Desarrollo Curricular | No | Competencia | PDF, DOCX |
| Plan Concertado | No | Plan (solo los que aún no tienen documento) | PDF, DOCX |

**Validaciones:** archivo obligatorio, tamaño máximo 20 MB, extensión restringida a `.pdf/.docx/.doc`; entidad relacionada obligatoria según el tipo; título obligatorio donde aplica.

**Permisos de edición/eliminación:** el botón solo aparece si el usuario es Administrador o es el instructor propietario del documento — el backend re-valida esto de forma independiente de la UI.

---

### 6.2 Guías (`Guias.razor`) — legacy

**Ruta:** `/guias` · **Acceso:** Administrador, Instructor

**Estado:** pantalla **prototipo/legacy** con datos de ejemplo hardcodeados y sin lógica de guardado real implementada. Funcionalmente ha sido **reemplazada por `Repositorio.razor`**, que cubre el mismo caso de uso con persistencia real, permisos y las 6 categorías completas.

> ⚠️ No documentada como módulo activo en `AGENTS.md` — se recomienda evaluar si debe eliminarse del menú de navegación para evitar confusión con Repositorio.

---

### 6.3 Monitorías (`MonitoriasAdmin.razor`)

**Ruta:** `/monitorias-admin` · **Acceso:** Administrador (control total, incluida asignación/desactivación de monitores), Instructor (vista filtrada a lo dirigido a él)

**Propósito:** Panel de administración de monitorías: sesiones vigentes, asistencias e informes recibidos de los monitores, y gestión de quién tiene perfil de monitor.

**Pestañas:**

1. **Monitorías vigentes** — listado de sesiones activas creadas por aprendices-monitores.
2. **Asistencias recibidas** — registros de asistencia que un monitor envió a un instructor destinatario (el instructor solo ve lo dirigido a él; el Administrador ve todo).
3. **Informes recibidos** — informes de sesión enviados por monitores.
4. **Monitores** — buscador de aprendices elegibles (En Formación, sin ser ya monitor activo) con botón "Asignar" (solo Administrador); listado de monitores con toggle de desactivación (solo Administrador).

**Relación con otros módulos:** el ciclo completo de monitorías se cierra en `Profile.razor` (pestaña "Gestión de Monitorías" del aprendiz-monitor, donde se crean las sesiones y se envían las asistencias/informes que aparecen aquí).

---

*Fin del manual de pantallas. Para el detalle de roles, permisos y diferencias de acceso entre Administrador, Instructor y Aprendiz, ver [`roles-permisos.md`](roles-permisos.md).*
