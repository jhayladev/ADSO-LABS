# ADSO Labs — Guía para Claude Code

> **Versión:** 4.0
> **Última actualización:** Julio 2026
> **Estado:** Documento vivo — debe reflejar fielmente el estado real del sistema.

Este archivo contiene **solo** el protocolo de comportamiento específico de Claude Code (sección 0). El resto del contenido del proyecto — arquitectura, stack, reglas de negocio, entidades — vive en [`AGENTS.md`](AGENTS.md), el archivo tool-agnostic que cualquier agente de IA (o persona) puede leer. Claude Code carga `AGENTS.md` automáticamente a través del import de abajo, así que no hace falta leerlo aparte ni duplicar su contenido aquí.

@AGENTS.md

---

## 0. Protocolo de Comportamiento del Agente

> Esta sección tiene **prioridad absoluta** sobre cualquier otra instrucción del documento. Se aplica en cada sesión, sin excepción.

### 0.1 Principio de mínima intervención

- Tocar **únicamente** los archivos directamente necesarios para la tarea solicitada.
- Si completar la tarea requiere modificar un archivo no obvio, **declararlo antes de actuar** y esperar confirmación.
- Está **prohibido** mejorar, limpiar, reorganizar o refactorizar código que no sea parte explícita de la tarea. Esto incluye: renombrar variables, extraer métodos, cambiar formato, reordenar usings, ajustar indentación en bloques no tocados.
- Si se detecta un problema adyacente (bug, inconsistencia, mejora potencial), **reportarlo en texto** al final de la respuesta, pero **no tocarlo**.

### 0.2 Protocolo antes de actuar

Antes de escribir o modificar cualquier código, el agente debe declarar:

```
Archivos a modificar:
- <ruta/archivo.cs> — <motivo puntual>

Archivos a leer (solo lectura):
- <ruta/archivo.cs> — <por qué se necesita como contexto>

Archivos que NO se tocarán (aunque parezcan relacionados):
- <ruta/archivo.cs>
```

Si el usuario no aprueba o corrige el plan, **no proceder**.

### 0.3 Manejo de incertidumbre

- Si algo no está claro — comportamiento esperado, entidad involucrada, regla de negocio aplicable — **preguntar antes de asumir**.
- No inventar implementaciones "razonables" para llenar vacíos de especificación.
- Si la tarea es ambigua, presentar las interpretaciones posibles y pedir que el usuario elija una.

### 0.4 Atomicidad de tareas

- Cada tarea es independiente y acotada. No encadenar tareas no solicitadas bajo el argumento de que "ya que estamos".
- Si se pide implementar X, no implementar Y aunque Y sea un prerequisito lógico obvio — reportarlo y esperar instrucción.

### 0.5 Cambios estructurales — requieren aprobación explícita

Los siguientes cambios **nunca** se realizan sin que el usuario los pida expresamente:

- Agregar, eliminar o modificar migraciones de EF Core.
- Cambiar nombres de entidades, propiedades o tablas.
- Modificar interfaces de `Application` (contratos).
- Reorganizar carpetas o namespaces.
- Cambiar la firma de métodos públicos.
- Agregar dependencias (NuGet packages).

### 0.6 Al terminar una tarea

Responder siempre con este formato:

```
✅ Hecho: <descripción de lo que se cambió>

Archivos modificados:
- <ruta/archivo.cs>

⚠️ Observaciones (sin tocar):
- <problema encontrado, si existe>
```

### 0.7 Router de Skills

**Paso obligatorio al inicio de cada tarea:** identificar el tipo de tarea y cargar la skill correspondiente antes de escribir cualquier código o propuesta.

| Si la tarea involucra… | Skill a cargar |
|------------------------|----------------|
| Añadir, mover o revisar código entre capas (`Core`, `Application`, `Infrastructure`, `API`, `Web`) | `.claude/skills/architecture.md` |
| Controllers, servicios, EF Core, autenticación, importación SOFIA, correo | `.claude/skills/backend.md` |
| Componentes Blazor, páginas `.razor`, `ApiService`, sesión, UI Bootstrap | `.claude/skills/blazor.md` |
| Solo documentación: `CLAUDE.md`, `AGENTS.md`, SRS, `docs/` | `.claude/skills/cowork.md` |
| Auditoría, diagnóstico, evaluación de cumplimiento del SRS | `.claude/skills/review-mode.md` |
| Issues, labels, milestones, GitHub Project | `.claude/skills/github.md` |

**Reglas del router:**

- Si la tarea toca múltiples áreas, cargar **todas** las skills relevantes antes de actuar.
- Si no existe skill para el tipo de tarea detectado, **preguntar al usuario antes de proceder**. No improvisar.
- Si hay duda sobre qué skill aplica, mencionar la duda y esperar confirmación — no asumir.
- Las skills tienen prioridad sobre el conocimiento general del agente. Si una skill contradice una suposición "razonable", la skill gana.
- Las skills viven en `.claude/skills/` y **sí están trackeadas en git** (a diferencia de `.claude/worktrees/` y `.claude/settings.local.json`, que son locales a cada máquina) — deberían existir en cualquier clon del repositorio.

---

*Este archivo es específico de Claude Code. Para la referencia canónica de arquitectura y reglas de negocio del proyecto, ver [`AGENTS.md`](AGENTS.md).*
