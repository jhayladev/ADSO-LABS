---
name: cowork
description: Reglas operativas para trabajar sobre el repositorio ADSO Labs en modo Cowork con alcance controlado. Define qué se puede y qué no se puede tocar, cómo se reportan los cambios y cómo se mantiene la trazabilidad.
---

# Skill · Cowork Mode — ADSO Labs

## 1. Propósito
Garantizar que cualquier intervención sobre el repositorio **ADSO Labs** se ejecute bajo un perímetro de acceso parcial, preservando la integridad del código fuente, migraciones y configuración sensible. Esta skill se activa cuando el usuario invoca explícitamente el modo Cowork o cuando existe un alcance delimitado únicamente a documentación, reglas de comportamiento y skills.

## 2. Contexto del proyecto
- Solución: `AdsoLabs.sln` con **5 proyectos** bajo Clean Architecture:
  - `AdsoLabs.Core` (dominio, entidades, enums)
  - `AdsoLabs.Application` (casos de uso, interfaces, DTOs)
  - `AdsoLabs.Infrastructure` (EF Core, SMTP, hashing PBKDF2, Identity)
  - `AdsoLabs.API` (Web API, autenticación por cookies, endpoints REST)
  - `AdsoLabs.Web` (Blazor Server, UI funcional)
- Autenticación oficial: **ASP.NET Core Authentication.Cookies** (esquema `AdsoLabs.Auth`). No se usa JWT.
- Hashing: **PBKDF2-SHA256** (100.000 iteraciones, salt 16B, hash 32B).
- Documentación viva: `CLAUDE.md` y `docs/ADSO_Labs_SRS.md` (ambas en v3.1).

## 3. Alcance permitido (whitelist)
El modo Cowork **solo** puede crear o modificar archivos dentro de:
- `CLAUDE.md` (raíz del repositorio).
- `docs/**` (SRS, guías técnicas, manuales operativos, ADRs).
- `.claude/skills/**` (este directorio y archivos hermanos).
- `README.md` en la raíz, siempre que se trate de documentación (nunca scripts ni automatizaciones destructivas).

## 4. Alcance prohibido (blacklist estricta)
Bajo ninguna circunstancia se deben modificar, crear o borrar:
- Código fuente `.cs` en `src/**` (Core, Application, Infrastructure, API, Web).
- Archivos `.razor`, `.razor.cs`, `.razor.css` de Blazor.
- Migraciones EF Core en `AdsoLabs.Infrastructure/Migrations/**`.
- `appsettings*.json`, `launchSettings.json`, cadenas de conexión, credenciales SMTP, secretos de usuario.
- Archivos de proyecto `.csproj`, `.sln`, `global.json`, `nuget.config`.
- Pipelines, workflows, Dockerfiles o scripts de despliegue.
- Datos seed, snapshots o archivos generados por EF Core (`*ModelSnapshot.cs`).

Si el usuario solicita tocar algo fuera del whitelist, **detener** y pedir confirmación explícita, explicando el impacto.

## 5. Reglas de comportamiento
1. **No re-analizar** lo ya revisado si el usuario indica ejecución directa; usar `CLAUDE.md` y el SRS v3.1 como fuente de verdad.
2. **No eliminar información válida**: siempre ampliar, corregir o reformular. Cualquier borrado debe justificarse en la sección de cambios.
3. **Consistencia terminológica**: respetar los términos canónicos del proyecto — `Ficha`, `FichaAprendiz`, `CompetenciaResultado`, `FichaCompetenciaResultado`, `JuicioResultado`, `Sesion`, `Asistencia`, `ImportacionArchivo`, `TipoArchivo`, `PasswordResetToken`, `TRASLADADO` (no `TRASLADO`).
4. **Estados SOFIA Plus** válidos: `EN FORMACION`, `CANCELADO`, `RETIRO VOLUNTARIO`, `TRASLADADO`.
5. **Marcar cumplimiento** en documentación de requisitos con `✅ cumplido`, `⚠️ parcial`, `❌ no implementado`.
6. **No inventar** endpoints, entidades ni servicios. Si algo no está en el código o en la documentación verificada, marcarlo como propuesta y etiquetarlo como `pendiente`.
7. **Preservar cross-references** en el SRS: si se elimina un RF, no renumerar los restantes; solo dejar la nota de derogación.
8. **Trazabilidad obligatoria**: al final de cada intervención entregar la lista de archivos tocados y el motivo del cambio.

## 6. Procedimiento estándar
1. Leer `CLAUDE.md` y el SRS actual antes de producir cambios.
2. Confirmar que el archivo objetivo está dentro del whitelist (sección 3).
3. Aplicar el cambio con el menor diff posible.
4. Validar que no se introdujeron referencias a JWT, BCrypt ni stacks derogados.
5. Informar al usuario qué se modificó, por qué, y entregar enlace/ruta del artefacto.

## 7. Señales para salir de Cowork
Si el usuario solicita explícitamente cambios de código, migraciones o configuración, esta skill se desactiva y debe:
- Responder que el modo Cowork no autoriza ese cambio.
- Proponer que el usuario levante el alcance (autorización explícita) o abra un flujo dev separado.
- Documentar la solicitud en la sección de pendientes del SRS si corresponde.

## 8. Checklist de salida
- [ ] Solo se tocaron archivos del whitelist.
- [ ] `CLAUDE.md` y SRS mantienen consistencia terminológica.
- [ ] Se listan los archivos modificados con su motivo.
- [ ] No se introdujeron referencias a tecnologías derogadas (JWT, BCrypt).
- [ ] Las skills en `.claude/skills/` no contradicen entre sí.
