# ADSO Labs — Roles y Permisos

> **Versión:** 1.0
> **Última actualización:** Julio 2026
> **Alcance:** Referencia detallada de los tres roles de usuario del sistema (`Administrador`, `Instructor`, `Aprendiz`), sus permisos, restricciones de acceso y diferencias entre ellos. Complementa [`manual-pantallas.md`](manual-pantallas.md) (qué hace cada pantalla) y [`AGENTS.md`](../AGENTS.md) (reglas de negocio y arquitectura).
> **Control de acceso real:** RBAC basado en claims de la cookie de sesión (`ClaimTypes.Role`), validado **en backend** vía `[Authorize(Roles = "...")]` en cada controller de `AdsoLabs.API` — no solo en la UI de Blazor. Los roles válidos son los literales exactos `Administrador`, `Instructor`, `Aprendiz` (§5 `AGENTS.md`).

---

## Índice

1. [Resumen comparativo](#1-resumen-comparativo)
2. [Administrador](#2-administrador)
3. [Instructor](#3-instructor)
4. [Aprendiz](#4-aprendiz)
5. [Casos especiales: Monitor y Patrocinado](#5-casos-especiales-monitor-y-patrocinado)
6. [Reglas transversales de autenticación aplicables a los tres roles](#6-reglas-transversales-de-autenticación-aplicables-a-los-tres-roles)

---

## 1. Resumen comparativo

| Pantalla / Módulo | Administrador | Instructor | Aprendiz |
|---|---|---|---|
| Login / Acceso / Activar cuenta / Recuperar contraseña | Público | Público | Público |
| Home (Dashboard) | ✅ Todas las fichas | ✅ Solo sus fichas | ❌ Sin acceso (tiene `/perfil`) |
| Perfil del Aprendiz (`/perfil`) | ❌ | ❌ | ✅ Propio |
| Perfil del Instructor (`/perfil-instructor/{id}`) | ✅ Ver y editar cualquiera | ✅ Ver cualquiera / editar solo el propio | ❌ |
| Fichas (detalle) | ✅ Todas | ✅ Solo asignadas | ❌ |
| Catálogo de Competencias | ✅ | ✅ | ❌ |
| Asignación de Competencias (programar) | ✅ Único con acceso | ❌ | ❌ |
| Gestión de Aprendices (cambiar estado) | ✅ Único con acceso | ❌ | ❌ |
| Gestión de Instructores (listar) | ✅ Alta + activar/desactivar | ✅ Solo consulta | ❌ |
| Registro de Instructores (alta) | ✅ Único con acceso | ❌ | ❌ |
| Importaciones (fichas/aprendices/juicios) | ✅ Único con acceso | ❌ | ❌ |
| Asistencias (programación, sesiones, registro) | ✅ Todas las fichas | ✅ Solo sus fichas | ❌ |
| Patrocinios (CRUD) | ✅ Crear/editar/desactivar | ✅ Solo lectura | ❌ |
| Patrocinados (seguimiento + horario) | ✅ | ✅ (puede asignar horario) | ❌ |
| Reportes (8 tipos) | ✅ Todas las fichas | ✅ Solo sus fichas / su propia carga | ❌ |
| Informe individual del aprendiz | ✅ | ✅ | ❌ (ve su propio avance desde `/perfil`) |
| Predicción IA (asignación + predicción) | ✅ + aprueba/rechaza solicitudes | ✅ Solicita asignaciones | ❌ |
| Repositorio documental | ✅ CRUD completo | ✅ Carga propia + ve todo, elimina solo lo suyo | ❌ (solo lectura/descarga desde fuera de esta pantalla) |
| Monitorías — panel admin (`/monitorias-admin`) | ✅ Asigna/desactiva monitores | ✅ Ve lo dirigido a él | ❌ |
| Monitorías — gestión propia (dentro de `/perfil`) | ❌ | ❌ | ✅ Solo si tiene `MonitorPerfil` activo |

---

## 2. Administrador

### 2.1 Descripción
Rol de control total del sistema. Es el único con acceso a las operaciones estructurales: importación masiva, alta de instructores, programación de competencias y cambio de estado de aprendices.

### 2.2 Permisos y acciones que puede ejecutar
- Importar fichas, aprendices y juicios evaluativos SOFIA Plus (`/importaciones`).
- Dar de alta instructores (`/registro-instructor`) y activar/desactivar cualquier instructor (`/instructor`).
- Configurar la ventana de programación de una competencia y programar/eliminar resultados de aprendizaje individuales (`/asignacion-competencias`) — acción exclusiva de este rol (§7.4 `AGENTS.md`).
- Cambiar el estado de cualquier aprendiz (`EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO`) desde `/aprendiz`.
- CRUD completo de Patrocinios (crear, editar, desactivar) — el instructor solo puede leer (§7.7 `AGENTS.md`).
- Ver y generar reportes/dashboard de **todas** las fichas del sistema, no solo las propias.
- CRUD completo del Repositorio documental (cualquier documento, no solo el propio).
- Asignar y desactivar el perfil de **Monitor** de cualquier aprendiz elegible, y aprobar/rechazar solicitudes de asignación de competencia generadas por instructores desde Predicción IA.
- Editar el contacto (correo/teléfono) de cualquier instructor.

### 2.3 Restricciones
- **Nunca se bloquea por intentos fallidos de login**, sin importar cuántos intentos fallidos acumule (§7.1 `AGENTS.md`) — excepción explícita respecto a los otros dos roles.
- No tiene un perfil académico propio (no es aprendiz ni instructor); no aparece en reportes de carga docente ni de progreso académico.

### 2.4 Módulos y pantallas a las que tiene acceso
Todos los del sistema excepto `/perfil` (exclusivo de Aprendiz). Ver tabla comparativa en la sección 1.

---

## 3. Instructor

### 3.1 Descripción
Responsable de la gestión pedagógica de las fichas que tiene asignadas: competencias, asistencia, aprendices y guías de sus propias competencias.

### 3.2 Permisos y acciones que puede ejecutar
- Ver el dashboard y reportes **filtrados a sus fichas asignadas** (el filtrado se aplica en la API vía `ObtenerIdUsuarioAutenticado()`, no confía en parámetros del cliente).
- Gestionar asistencia (programación, sesiones, registro) de sus fichas.
- Cargar guías, planeaciones, proyectos formativos, instrumentos de evaluación y desarrollo curricular al Repositorio; ve todos los documentos del sistema, pero **solo puede cambiar de estado o eliminar los que él mismo subió**.
- Solicitar la asignación de una competencia sugerida por Predicción IA (crea una `SolicitudAsignacion` en estado Pendiente, que el Administrador debe aprobar o rechazar).
- Editar su propio contacto (correo/teléfono) en `/perfil-instructor/{su-id}`.
- Ver el módulo de Patrocinios y Patrocinados en modo **solo lectura** (puede asignar horario de patrocinio desde Patrocinados, pero no crear/editar/desactivar patrocinios).
- Consultar el panel de Monitorías (`/monitorias-admin`) filtrado a lo que los monitores le dirigieron a él específicamente.

### 3.3 Restricciones
- **No puede** programar competencias (`/asignacion-competencias` requiere rol Administrador exclusivamente).
- **No puede** cambiar el estado de un aprendiz ni acceder a `/aprendiz` (requiere Administrador).
- **No puede** importar fichas, aprendices ni juicios evaluativos (`/importaciones` requiere Administrador).
- **No puede** dar de alta otros instructores ni activar/desactivar cuentas de instructores.
- **No puede** crear, editar ni desactivar patrocinios — solo consulta y asignación de horario.
- **No puede** asignar ni desactivar el perfil de Monitor de un aprendiz.
- **Sí se bloquea** tras 5 intentos fallidos de login en 15 minutos (bloqueo de 30 min, igual que Aprendiz).

### 3.4 Módulos y pantallas a las que tiene acceso
Home, Perfil de Instructor (propio y de otros, en lectura), Fichas (solo asignadas), Catálogo de Competencias, Gestión de Instructores (solo lectura), Asistencias/Sesiones/Registro (solo sus fichas), Patrocinios (lectura) y Patrocinados, Reportes e Informe Individual (solo sus fichas), Predicción IA, Repositorio, Monitorías (panel admin, filtrado).

---

## 4. Aprendiz

### 4.1 Descripción
Rol de autogestión: perfil propio, sus competencias, su cronograma y sus notificaciones. Puede además ser **monitor** de una competencia — no es un rol distinto, es un estado adicional (`MonitorPerfil`) sobre el mismo usuario Aprendiz.

### 4.2 Permisos y acciones que puede ejecutar
- Ver y editar (parcialmente) su propio perfil: datos de contacto, acudiente, contacto de emergencia (`/perfil`).
- Consultar su cronograma de sesiones (calendario, lista, próximas, historial) y su avance por competencia con juicios.
- Ver sus propias observaciones registradas por instructores (solo lectura, no puede crearlas ni editarlas).
- Inscribirse/desinscribirse de sesiones de monitoría vigentes de otros aprendices-monitores.
- **Si tiene `MonitorPerfil` activo:** crear sus propias sesiones de monitoría, ver inscritos, pasar asistencia de los inscritos a un instructor destinatario y enviar informe de la sesión.
- Cambiar su propia contraseña (primer login o recuperación) aceptando el consentimiento informado de tratamiento de datos en el primer login.

### 4.3 Restricciones
- **No tiene acceso** a ningún módulo administrativo o pedagógico de gestión: Fichas, Competencias, Asistencias (gestión), Instructores, Importaciones, Reportes, Repositorio, Predicción IA ni Monitorías (panel admin) están fuera de su alcance — intentar acceder debe resultar en rechazo de autorización en backend, no solo ocultamiento de UI.
- **No puede** editar su documento de identidad, tipo de documento, ni su propio estado (`EN FORMACION`/`CANCELADO`/etc.) — esos campos son de solo lectura o gestionados exclusivamente por Administrador.
- **No puede** registrarse individualmente por UI — todo aprendiz ingresa al sistema por importación SOFIA Plus (§7.9 `AGENTS.md`); el primer acceso se hace vía `/activar-cuenta` con su número de documento.
- **Sí se bloquea** tras 5 intentos fallidos de login en 15 minutos (30 min de bloqueo).
- Un aprendiz con estado distinto de `EN FORMACION` **no aparece** en listas de asistencia y, si su estado es Cancelado/Retiro Voluntario/Trasladado, es redirigido fuera de pantallas que muestren su informe individual.
- El cambio de estado a `CANCELADO` o `RETIRO VOLUNTARIO` (ejecutado por un Administrador) sincroniza automáticamente `Usuario.Activo = false`, impidiéndole iniciar sesión.

### 4.4 Módulos y pantallas a las que tiene acceso
Login/Acceso/Activación/Recuperación de contraseña, y `/perfil` (con sus 7 pestañas, incluida "Gestión de Monitorías" condicionada a ser monitor activo).

---

## 5. Casos especiales: Monitor y Patrocinado

### 5.1 Monitor
- **No es un rol de `Usuario`/`Rol` distinto** — sigue siendo un usuario con rol `Aprendiz` que además tiene un registro `MonitorPerfil` activo asociado a una competencia.
- Se activa/desactiva exclusivamente por el **Administrador** desde `/monitorias-admin` → pestaña "Monitores", buscando aprendices elegibles (estado `EN FORMACION`, sin ser ya monitor activo).
- Mientras el `MonitorPerfil` está activo, el aprendiz ve una pestaña adicional en su propio perfil ("Gestión de Monitorías") donde crea sesiones, gestiona inscritos, pasa asistencia y envía informes a un instructor destinatario.
- El `MonitoriaController` de la API **sirve a los tres roles a la vez** (aprendices se inscriben a sí mismos, instructores/administrador consultan y asignan) sin una separación de endpoints 1:1 por rol — punto señalado como hallazgo técnico en `README.md`, no una limitación de negocio.

### 5.2 Patrocinado
- Tampoco es un rol distinto: es un aprendiz (`EN FORMACION`) con un registro `Patrocinio` activo en etapa Lectiva o Productiva.
- La gestión del patrocinio (crear/editar/desactivar) es exclusiva de **Administrador**; el **Instructor** solo tiene lectura y puede asignar/eliminar el horario de patrocinio desde `/asistencias-patrocinados`.
- Un aprendiz patrocinado solo puede tener una etapa activa a la vez (§7.7 `AGENTS.md`).

---

## 6. Reglas transversales de autenticación aplicables a los tres roles

Estas reglas gobiernan el acceso de cualquier rol y están implementadas en `AuthService`/`AuthController` (ver también §7.1 de `AGENTS.md`):

- **Primer login:** tanto instructores como aprendices (incluido el Administrador sembrado inicialmente) usan su número de documento como contraseña temporal. El sistema detecta `PrimerLogin = true` y compara contra el documento en lugar del hash PBKDF2. Tras el cambio de contraseña obligatorio, `PrimerLogin` pasa a `false`.
- **Mensajes de error de login uniformes:** nunca se distingue "usuario no existe" de "contraseña incorrecta" — siempre "Usuario o contraseña incorrectos", para no filtrar qué documentos/correos existen en el sistema.
- **Bloqueo de cuenta:** 5 intentos fallidos en 15 minutos → bloqueo de 30 minutos, con desbloqueo automático al siguiente intento pasado ese tiempo. **Excepción única: el Administrador nunca se bloquea**, sin importar los intentos.
- **`Usuario.Activo = false`** impide el inicio de sesión bajo cualquier circunstancia, independientemente del rol.
- **Autenticación Web↔API:** invisible para los tres roles — el navegador solo ve la cookie `AdsoLabs.Auth`; el JWT interno de 2 minutos que autentica cada llamada de `AdsoLabs.Web` hacia `AdsoLabs.API` se minta automáticamente a partir de la cookie ya validada, sin intervención del usuario.
- **Auditoría:** cada login exitoso (de cualquier rol) actualiza `Usuario.UltimoAcceso`; los intentos fallidos incrementan `IntentosFallidos`, también para los tres roles por igual (salvo que, para Administrador, ese contador nunca deriva en bloqueo real).

---

*Fin del documento de roles y permisos. Para el detalle funcional de cada pantalla mencionada aquí, ver [`manual-pantallas.md`](manual-pantallas.md).*
