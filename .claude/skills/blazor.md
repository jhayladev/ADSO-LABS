---
name: blazor
description: Lineamientos para el frontend Blazor Server de ADSO Labs. Define la estructura de componentes, handlers Razor Pages, consumo de API, sesión por cookies y patrones de UI Bootstrap 5.
---

# Skill · Blazor Server — ADSO Labs

## 1. Propósito
Estandarizar el desarrollo de componentes y páginas en `AdsoLabs.Web` respetando Clean Architecture, la autenticación por cookies y las decisiones de UX del proyecto.

## 2. Stack frontend oficial
- **Framework**: Blazor Server (.NET 9) con render mode `InteractiveServer`.
- **Hosting**: ASP.NET Core embebido (mismo host que servicios Web, no SSR estático).
- **UI**: Bootstrap 5 (`wwwroot/lib/bootstrap`).
- **Consumo de API**: `HttpClient` tipado por módulo (`AuthApiService`, `AprendizApiService`, `FichaApiService`, etc.).
- **Autenticación**: cookie compartida con la API (`AdsoLabs.Auth`). Esquema idéntico en ambos proyectos.
- **Estado de sesión**: `SesionService` scoped por circuito (`IdUsuario`, `Rol`, `PrimerLogin`).
- **Reportes PDF**: generación client-side con jsPDF sobre `Informe.razor`.

## 3. Estructura del proyecto `AdsoLabs.Web`
```
AdsoLabs.Web/
├── Components/
│   ├── Layout/                 → MainLayout.razor, LoginLayout.razor, NavMenu.razor
│   └── Pages/                  → Una página .razor por módulo (Fichas.razor, Aprendices.razor, ...)
├── Pages/                      → Razor Pages handlers: LoginHandler, LogoutHandler, ActivacionHandler, RecuperacionHandler
├── Services/
│   ├── SesionService.cs
│   └── Api/                    → Un *ApiService.cs por módulo
└── wwwroot/                    → app.css, images/, lib/bootstrap/
```

Importante: los archivos en `Pages/` **no son componentes Blazor**: son Razor Pages handlers que manejan el flujo cookie-based (login, logout, activación, recuperación). No confundir con `Components/Pages/`.

## 4. Patrón para nuevas páginas
1. Crear `NombreModulo.razor` en `Components/Pages/`.
2. Registrar la ruta con `@page "/nombre-modulo"`.
3. Declarar `@inject NombreModuloApiService` y `@inject SesionService`.
4. Exigir autenticación con `@attribute [Authorize(Roles = "...")]` cuando corresponda.
5. Crear `NombreModuloApiService.cs` en `Services/Api/` que encapsule los llamados al backend.
6. Si se necesita CSS específico, agregar `NombreModulo.razor.css` (scoped).
7. Evitar lógica de negocio en el `.razor`: delegar al `ApiService` o a servicios de Application a través de la API.

## 5. Consumo de la API
- `HttpClient` configurado en `Program.cs` con `BaseAddress` desde `appsettings`.
- Los `*ApiService.cs` son clases scoped que reciben `HttpClient` por DI.
- Respetar los endpoints canónicos (ver `CLAUDE.md` §8). No duplicar rutas ni inventar nuevas.
- Serializar/deserializar con `System.Text.Json`.
- Propagar cookies automáticamente (`HttpClientHandler { UseCookies = true }`).
- Manejar respuestas `401 Unauthorized` redirigiendo a `/login` y `403 Forbidden` mostrando mensaje de permisos.

## 6. Sesión y control de acceso
- `SesionService` contiene los datos del usuario autenticado del circuito actual.
- Validación de roles en UI se hace con `<AuthorizeView Roles="Administrador">` o `@attribute [Authorize]`.
- **Regla clave**: la validación de autorización se hace SIEMPRE en backend. El control en UI es solo para UX.
- Si `PrimerLogin = true`, redirigir a la pantalla de cambio obligatorio de contraseña.
- Si el usuario pertenece al rol `Aprendiz` con estado `Activo = false`, cerrar sesión.

## 7. UI y estilos
- Framework de estilos: Bootstrap 5. No se usa Tailwind ni MudBlazor.
- Componentes reutilizables comunes: `TablaDatos`, `ModalConfirmacion`, `AlertaEstado`, `CargaArchivo`.
- Estados visuales de cumplimiento (para dashboards de reglas): `✅ cumplido`, `⚠️ parcial`, `❌ no implementado`.
- Accesibilidad mínima: etiquetas `aria-*`, foco visible, contraste adecuado.
- El dashboard debe ser responsive (desktop, tablet, móvil).

## 8. Reglas de negocio visibles en UI
- Aprendices con estado `≠ EN FORMACION` **no aparecen** en listados de asistencia.
- Competencias `Clausurada` siguen visibles en catálogo pero no seleccionables al asignar.
- El módulo `Patrocinio` es **solo lectura para instructores**; administradores pueden editar.
- En el perfil del aprendiz (cuando se implemente), solo son editables `Telefono` y `Direccion`.
- El informe académico se genera con jsPDF a partir de `Informe.razor` — actualmente con datos mock.

## 9. Reglas de comportamiento (IA)
1. No consumir `Infrastructure` ni `Application` directamente desde `Web`. Siempre vía API.
2. No duplicar DTOs: los modelos del frontend deben reflejar los DTOs de `Application`.
3. No mezclar lógica de importación SOFIA ni hashing en `Web`; es responsabilidad de backend.
4. No usar `localStorage`/`sessionStorage` para datos sensibles; el estado de sesión va por cookie + `SesionService`.
5. No introducir dependencias JS externas sin confirmar con el usuario.
6. No generar artefactos PDF server-side en el frontend; la integración actual es jsPDF cliente.
7. Respetar los nombres canónicos de rutas y parámetros — si un endpoint no existe, reportarlo antes de implementarlo.
8. Toda página nueva debe registrar su archivo en `NavMenu.razor` con el rol correcto.

## 10. Checklist frontend
- [ ] Página en `Components/Pages/`, ruta declarada con `@page`.
- [ ] `ApiService` correspondiente en `Services/Api/`.
- [ ] `[Authorize]` aplicado según rol.
- [ ] Consumo exclusivo por `HttpClient` tipado.
- [ ] Sin referencias a `AdsoLabs.Application`, `AdsoLabs.Infrastructure` ni `AdsoLabs.Core`.
- [ ] Estado visual respeta convenciones (Bootstrap, accesibilidad, responsive).
- [ ] UI no asume autorización: siempre validada también en backend.
