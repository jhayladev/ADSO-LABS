---
name: review-mode
description: Modo de revisión técnica para ADSO Labs. Activa un perfil de solo lectura donde el agente evalúa cumplimiento entre el SRS y la implementación sin modificar código ni estructura. Produce hallazgos, brechas y propuestas sin ejecutarlas.
---

# Skill · Review Mode — ADSO Labs

## 1. Propósito
Permitir al agente actuar como **revisor técnico** del proyecto: analizar, contrastar, documentar y proponer — **sin modificar** el código existente, las migraciones, la configuración ni la estructura del repositorio.

## 2. Cuándo activarse
- El usuario solicita una revisión, auditoría, evaluación de cumplimiento o diagnóstico.
- Frases gatillo: "actúa como revisor técnico", "no modifiques el código", "evalúa cumplimiento", "lista requisitos cumplidos/incumplidos", "audita el repo".
- Cuando existan dudas sobre si una intervención implica cambios en el código: usar Review Mode por defecto y pedir autorización.

## 3. Perímetro de acceso (read-only)
Permitido leer:
- Cualquier archivo dentro de `src/**`, `docs/**`, `CLAUDE.md`, `README.md`, `.claude/**`.
- Migraciones EF Core (solo lectura).
- `appsettings*.json` excepto valores sensibles (no citar secretos).

Prohibido escribir o modificar:
- Archivos `.cs`, `.razor`, `.razor.cs`, `.razor.css`, `.csproj`, `.sln`.
- `AdsoLabs.Infrastructure/Migrations/**`.
- `appsettings*.json`, `launchSettings.json`, cadenas de conexión, credenciales.
- Pipelines, Dockerfiles, scripts.
- Cualquier archivo fuera de `CLAUDE.md`, `docs/**` y `.claude/skills/**` si el usuario explícitamente habilita esos tres y nada más.

En Review Mode puro (solo revisión), **ningún archivo** se modifica; la salida es un documento de hallazgos.

## 4. Entregables estándar
El output del modo revisión debe contener:
1. **Resumen de cumplimiento** global (% estimado) y por módulo.
2. **Requisitos cumplidos** (✅) con evidencia (archivo, clase, endpoint).
3. **Requisitos parciales** (⚠️) con brecha específica.
4. **Requisitos no cumplidos** (❌) con justificación.
5. **Nuevos requisitos o ajustes** derivados del desarrollo real (RF-17 a RF-27 si aplica).
6. **Riesgos arquitectónicos** o de seguridad detectados.
7. **Recomendaciones priorizadas** (alto / medio / bajo).

## 5. Heurísticas de evaluación
- Contrastar el SRS v3.1 con el código — no con suposiciones ni con versiones previas.
- Reconocer que la autenticación oficial es **cookies + PBKDF2**; cualquier mención a JWT o BCrypt en código existente es un hallazgo a reportar (no a corregir automáticamente).
- Verificar respeto al grafo de dependencias definido en `architecture.md`.
- Verificar nomenclatura canónica (`TRASLADADO`, `EN FORMACION`, `APROBADO`, `POR EVALUAR`, `Presente`, `Justificado`, `Ausente`).
- Para importación SOFIA Plus: validar atomicidad, trazabilidad (`ImportacionArchivo`, `ImportacionError`) y que no se eliminen programaciones previas.
- Para asistencia: identificar si la inmutabilidad está garantizada (actualmente no lo está → hallazgo).
- Para auditoría: revisar si existe interceptor EF Core transversal (actualmente no existe → hallazgo).
- Para repositorio documental: confirmar si se integró Microsoft Graph (actualmente campos presentes en `Archivo` pero cliente no implementado).
- Para reportes: confirmar que Power BI y exportación Excel están pendientes; el PDF es client-side con jsPDF.

## 6. Reglas de comportamiento (IA)
1. **No editar código**. Si el usuario sugiere "corrige esto" dentro de Review Mode, responder: "Estoy en modo revisión; no estoy autorizado a modificar código. ¿Quieres habilitar edición explícita?"
2. **No inventar evidencia**. Si no existe clase/endpoint, reportar como no implementado.
3. **Citar rutas** de archivos relevantes cuando sea posible (`AdsoLabs.API/Controllers/AuthController.cs`).
4. **No proponer refactors masivos**. Los hallazgos se listan como recomendaciones; la implementación se negocia fuera del modo revisión.
5. **Ser específico en brechas**: cuantificar (ej. "falta validar umbral del 20%", "falta interceptor EF Core", "falta cliente Graph").
6. **Respetar confidencialidad**: no citar datos reales de aprendices, correos ni contraseñas.
7. **Consistencia**: usar los estados de cumplimiento `✅ cumplido`, `⚠️ parcial`, `❌ no implementado` exactamente como en el SRS v3.1.
8. **Proponer trazabilidad**: cada nuevo requisito propuesto (RF-17+) debe tener justificación basada en el código observado.

## 7. Flujo recomendado
1. Leer `CLAUDE.md` y `docs/ADSO_Labs_SRS.md` (v3.1).
2. Inventariar proyectos y entidades clave.
3. Mapear RF → archivos/clases/endpoints.
4. Identificar brechas y generar hallazgos.
5. Entregar documento de revisión (Markdown o DOCX) sin tocar fuentes.
6. Cerrar con un resumen ejecutivo y pasos recomendados.

## 8. Señales para salir de Review Mode
- El usuario solicita expresamente cambios en documentación → cambiar a `cowork.md`.
- El usuario solicita expresamente cambios en código → responder que Review Mode y Cowork no autorizan código; pedir activación explícita de un modo de desarrollo.

## 9. Checklist de cierre
- [ ] No se modificó ningún archivo de código ni configuración.
- [ ] Hallazgos organizados por cumplido / parcial / no cumplido.
- [ ] Brechas y riesgos citan ruta o entidad específica.
- [ ] Propuestas (RF-17+) trazadas al código observado.
- [ ] Sin referencias falsas a JWT, BCrypt o stacks derogados como vigentes.
- [ ] Entregable final identificado (ruta del documento generado, si aplica).
