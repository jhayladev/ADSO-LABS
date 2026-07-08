---
name: github
description: Lineamientos para gestionar el repositorio GitHub de ADSO Labs — issues, labels, milestones, projects y configuración general. Aplica cuando la tarea involucra organización del repositorio, trazabilidad de requisitos o gestión de trabajo pendiente.
---

# Skill · GitHub — ADSO Labs

## 1. Propósito
Estandarizar la gestión del repositorio GitHub de ADSO Labs para que refleje fielmente el estado real del proyecto, trazado directamente desde el SRS v3.1 y el CLAUDE.md v3.2. Esta skill se activa ante cualquier tarea que involucre issues, labels, milestones, projects o configuración del repositorio.

## 2. Cuándo activarse
- El usuario solicita crear, actualizar o cerrar issues.
- El usuario pide organizar el tablero de proyecto (GitHub Project).
- El usuario solicita configurar labels o milestones.
- Frases gatillo: "crea los issues", "actualiza el tablero", "refleja el estado en GitHub", "organiza el repo", "agrega la tarea al proyecto".

## 3. Repositorio oficial
- **Repositorio:** `JuanPabloMendozaLopez/adso-labs`
- **Rama principal:** `main`
- **Herramienta:** CLI `gh` (GitHub CLI). Verificar autenticación con `gh auth status` antes de operar.
- **Project:** `ADSO Labs — Roadmap` (board con columnas: `Backlog`, `En Progreso`, `En Revisión`, `Hecho`)

## 4. Protocolo obligatorio antes de actuar

**Nunca ejecutar comandos `gh` sin antes:**

1. **Auditar el código fuente** (Paso 0) — verificar artefactos reales antes de determinar el estado de cada issue. Ver sección 6.
2. **Consultar el estado actual del repo** — ejecutar `gh issue list --state all`, `gh label list`, `gh milestone list` para evitar duplicados.
3. **Presentar el plan completo** al usuario con la tabla de acciones a tomar.
4. **Esperar confirmación explícita** antes de ejecutar cualquier comando que modifique el repositorio.

## 5. Sistema de labels

### Labels de módulo (color `#0075ca`)
| Nombre | RFs asociados |
|--------|--------------|
| `modulo: autenticacion` | RF-02, RF-03, RF-04 |
| `modulo: dashboard` | RF-05, RF-25 |
| `modulo: fichas` | RF-07, RF-08, RF-09, RF-29 |
| `modulo: competencias` | RF-10 |
| `modulo: aprendices` | RF-12, RF-28 |
| `modulo: importacion-sofia` | RF-11, RF-17 |
| `modulo: repositorio` | RF-13, RF-20, RF-22, RF-26 |
| `modulo: asistencia` | RF-14, RF-18 |
| `modulo: patrocinio` | RF-15, RF-23 |
| `modulo: reportes` | RF-16 |
| `modulo: infraestructura` | RF-19, RF-21, RF-24, RF-27 |
| `modulo: perfil-aprendiz` | RF-06 |

### Labels de estado
| Nombre | Color | Significado |
|--------|-------|-------------|
| `estado: cumplido` | `#0e8a16` | Verificado en código — issue se cierra |
| `estado: parcial` | `#e4a50a` | Implementación incompleta — issue abierto |
| `estado: pendiente` | `#d93f0b` | Sin implementar — issue abierto |

### Labels de prioridad
| Nombre | Color |
|--------|-------|
| `prioridad: alta` | `#b60205` |
| `prioridad: media` | `#e4a50a` |
| `prioridad: baja` | `#0e8a16` |

### Labels de tipo
| Nombre | Color |
|--------|-------|
| `tipo: bug` | `#d73a4a` |
| `tipo: feature` | `#a2eeef` |
| `tipo: deuda-tecnica` | `#cfd3d7` |
| `tipo: documentacion` | `#fef2c0` |

### Labels de capa arquitectónica (color `#f9d0c4`)
`capa: core` · `capa: application` · `capa: infrastructure` · `capa: api` · `capa: web`

### Label especial
| Nombre | Color | Uso |
|--------|-------|-----|
| `no-tocar-sin-aprobacion` | `#000000` | Migraciones, HashService, ImportadorJuicios, handlers auth |

## 6. Auditoría de código (Paso 0)

Antes de crear issues, verificar la existencia de los siguientes artefactos por capa:

| Artefacto | Ruta | Indica |
|-----------|------|--------|
| Página Blazor | `AdsoLabs.Web/Components/Pages/<Modulo>.razor` | UI implementada (verificar que no sea stub) |
| ApiService | `AdsoLabs.Web/Services/Api/<Modulo>ApiService.cs` | Consumo de API desde frontend |
| Controlador | `AdsoLabs.API/Controllers/<Modulo>Controller.cs` | Endpoint backend |
| AppService | `AdsoLabs.Application/Services/<Modulo>AppService.cs` | Caso de uso implementado |
| Entidad | `AdsoLabs.Core/Entities/<Entidad>.cs` | Modelo de dominio |
| Migración | `AdsoLabs.Infrastructure/Migrations/` | Esquema en BD |
| Repositorio | `AdsoLabs.Infrastructure/Repositories/<Entidad>Repository.cs` | Persistencia |

**Criterio de stub:** un archivo `.razor` con menos de 20 líneas que solo contiene un `<h3>` o similar se considera stub — el issue va como `estado: pendiente`.

**Resultado del Paso 0:** presentar tabla con formato:
```
| Issue | Estado encontrado en código | Acción a tomar |
|-------|-----------------------------|----------------|
| [RF-XX] Título | ✅/⚠️/❌ descripción | Crear abierto / Crear cerrado / Omitir |
```

## 7. Milestones oficiales

| Nombre | Descripción | Issues asociados |
|--------|-------------|-----------------|
| `M1 - Núcleo académico completo` | RF-06, RF-12, RF-14, RF-05 dashboard | 4 semanas desde fecha actual |
| `M2 - Repositorio documental` | RF-13, RF-20, RF-08 cronograma | 8 semanas desde fecha actual |
| `M3 - Reportes y auditoría` | RF-16, auditoría transversal | 12 semanas desde fecha actual |
| `M4 - Deuda técnica y hardening` | Inmutabilidad, política contraseñas, RLS | 16 semanas desde fecha actual |

## 8. Estructura de un issue

Cada issue debe contener en su cuerpo:

```markdown
## Requisito
**RF-XX** — [nombre del requisito según SRS v3.1]
Sección SRS: §X.X

## Descripción
[Máximo 2 oraciones parafraseadas del SRS. No copiar textual.]

## Estado encontrado en código
[Resultado del Paso 0: qué artefacto existe o no existe y en qué ruta]

## Criterio de cierre
[Condición verificable que indica que el issue está resuelto]
```

## 9. Gestión del GitHub Project

- **Board:** `ADSO Labs — Roadmap`
- Issues abiertos → columna `Backlog`
- Issues cerrados por implementados → columna `Hecho`
- Issues en desarrollo activo → columna `En Progreso`
- Issues en revisión/PR → columna `En Revisión`
- Si el project ya existe, no crear uno nuevo — agregar solo los issues faltantes.

## 10. Decisiones de requisitos registradas

| Decisión | Fecha | Detalle |
|----------|-------|---------|
| RF-05 y RF-16 sin Power BI | May 2026 | Dashboard y reportes se implementan directamente en Blazor + API. El iframe placeholder se elimina. No usar Power BI Embedded. |

> Toda nueva decisión que cambie un requisito debe agregarse aquí antes de crear o cerrar el issue correspondiente.

## 11. Reglas de comportamiento (IA)

1. Nunca crear issues fuera de la lista oficial sin que el usuario los pida explícitamente.
2. Nunca duplicar labels, milestones o issues — consultar estado actual del repo primero.
3. El estado (`estado: cumplido/parcial/pendiente`) se determina por el Paso 0, nunca por el SRS solo.
4. Si un comando `gh` falla, reportar el error exacto y esperar instrucción. No intentar workarounds.
5. No modificar archivos del repositorio — esta skill opera exclusivamente con `gh`.
6. Ante un cambio de requisito (como RF-05/RF-16), cerrar los issues afectados con comentario explicativo antes de crear los nuevos.
7. Toda decisión arquitectónica o de producto que cambie un RF debe quedar registrada en la sección 10 de esta skill y en el CLAUDE.md.

## 12. Checklist de cierre

- [ ] `gh auth status` verificado antes de operar.
- [ ] Estado actual del repo consultado (no se duplicó nada).
- [ ] Paso 0 completado y confirmado por el usuario.
- [ ] Issues creados con cuerpo completo (RF, descripción, estado en código, criterio de cierre).
- [ ] Labels y milestones asignados correctamente.
- [ ] Project actualizado con columnas correctas.
- [ ] Decisiones de requisitos registradas en sección 10.
- [ ] Formato de respuesta final entregado:
```
✅ Hecho
Labels creadas: X (X omitidas por existentes)
Milestones creados: X
Issues creados: X (X abiertos, X cerrados)
Project: [nombre] — [URL]
⚠️ Observaciones: [errores o decisiones tomadas]
```
