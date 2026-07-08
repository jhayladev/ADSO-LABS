# Pruebas de carga (RNF-03)

> Resuelve el issue #114. **Alcance de esta entrega: solo los scripts k6 y esta guía.**
> No se ejecutó ninguna corrida de 50 VUs contra la base de datos local — se decidió
> así explícitamente para no generar carga ni datos de prueba en el ambiente de
> desarrollo compartido. Ejecutar estos scripts (y ajustar los thresholds si hace
> falta) queda como siguiente paso, contra un ambiente dedicado a pruebas.

## Qué se prueba y por qué

`AdsoLabs.API` es el componente que realmente ejecuta las consultas a SQL Server —
es el punto donde un P95 alto importaría (EF Core, joins, etc.). Por eso los 4
escenarios apuntan directo a la API (no a `AdsoLabs.Web`), replicando exactamente el
patrón de autenticación interna que ya usa el sistema: `AdsoLabs.Web` mintea un JWT
corto (2 min) a partir de la cookie del usuario y lo manda como `Authorization: Bearer`
en cada llamada a la API (ver `AdsoLabs.Web/Services/InternalTokenHandler.cs`). Los
scripts hacen lo mismo — `helpers/jwt.js` reproduce ese mismo token, firmado con la
misma clave (`InternalAuth:Key`) del ambiente que se está probando.

**Importante:** esto significa que los scripts necesitan la clave secreta de
`InternalAuth:Key` del ambiente contra el que se corren. Nunca commitear esa clave;
siempre pasarla por variable de entorno (`-e`) al momento de correr k6.

## Escenarios

| Script | Endpoint | Necesita token |
|---|---|---|
| `login.js` | `POST /api/login` | No (`[AllowAnonymous]`) |
| `dashboard.js` | `GET /api/mi-dashboard` | Sí |
| `fichas.js` | `GET /api/obtener-fichas` | Sí |
| `asistencia.js` | `GET /api/obtener-resultados-programados-ficha` | Sí |

Los 4 usan el mismo perfil de carga: rampa a 50 VUs en 30s, sostenido 2 minutos,
rampa a 0 en 30s. Umbrales (`thresholds`): `p(95) < 2000ms` y `< 1%` de requests
fallidos — si no se cumplen, k6 termina con código de salida distinto de cero.

## Requisitos

- [k6](https://k6.io/docs/get-started/installation/) instalado.
- La API corriendo y accesible (local o en un ambiente de pruebas dedicado).
- Un usuario real de prueba para `login.js` (correo + password, **no** el
  admin/instructor real de producción).
- Los datos (`id_usuario`, rol, `id_instructor`, número de ficha) de un usuario y
  ficha que realmente existan en la base contra la que se corre — los scripts no
  crean datos, solo leen.

## Variables de entorno

| Variable | Usado por | Descripción |
|---|---|---|
| `API_BASE_URL` | todos | Ej. `https://localhost:7221`. Default: `https://localhost:7221`. |
| `INTERNAL_AUTH_KEY` | dashboard, fichas, asistencia | Mismo valor que `InternalAuth:Key` en el `appsettings.Development.json` de la API del ambiente probado. |
| `INTERNAL_AUTH_ISSUER` | dashboard, fichas, asistencia | Default: `AdsoLabs.Web` (debe coincidir con `InternalAuth:Issuer` de la API). |
| `INTERNAL_AUTH_AUDIENCE` | dashboard, fichas, asistencia | Default: `AdsoLabs.API` (debe coincidir con `InternalAuth:Audience` de la API). |
| `TEST_ID_USUARIO` | dashboard, fichas, asistencia | `id_usuario` de un usuario real (Administrador o Instructor) del ambiente probado. |
| `TEST_ROL` | dashboard, fichas, asistencia | `Administrador` o `Instructor`, según el usuario elegido. |
| `TEST_ID_INSTRUCTOR` | dashboard, fichas, asistencia | `id_instructor` del usuario, si aplica (0 si es Administrador sin perfil de instructor). |
| `TEST_NUMERO_FICHA` | asistencia | Número de una ficha real con resultados programados. |
| `TEST_CORREO` / `TEST_PASSWORD` | login | Credenciales de un usuario de prueba real. |

## Cómo correr

```bash
# Login (no necesita InternalAuth)
k6 run tests/load/login.js \
  -e API_BASE_URL=https://localhost:7221 \
  -e TEST_CORREO=usuario.prueba@sena.edu.co \
  -e TEST_PASSWORD=laPasswordReal

# Dashboard
k6 run tests/load/dashboard.js \
  -e API_BASE_URL=https://localhost:7221 \
  -e INTERNAL_AUTH_KEY=laMismaClaveDeAppsettingsDevelopment \
  -e TEST_ID_USUARIO=1 \
  -e TEST_ROL=Administrador

# Fichas
k6 run tests/load/fichas.js \
  -e API_BASE_URL=https://localhost:7221 \
  -e INTERNAL_AUTH_KEY=laMismaClaveDeAppsettingsDevelopment \
  -e TEST_ID_USUARIO=1 \
  -e TEST_ROL=Administrador

# Asistencia (requiere una ficha real con resultados programados)
k6 run tests/load/asistencia.js \
  -e API_BASE_URL=https://localhost:7221 \
  -e INTERNAL_AUTH_KEY=laMismaClaveDeAppsettingsDevelopment \
  -e TEST_ID_USUARIO=1 \
  -e TEST_ROL=Administrador \
  -e TEST_NUMERO_FICHA=2930468
```

## Fuera de alcance de esta entrega

- No se corrió ninguno de estos scripts contra un ambiente real — falta esa corrida
  y el análisis de resultados (P95 real, cuellos de botella si los hay).
- No se prueba `AdsoLabs.Web` directamente: Blazor Server usa SignalR (WebSocket)
  para la interacción real, que k6 vainilla no simula de forma representativa
  (existe `xk6-browser` para eso, pero es una herramienta aparte, no cubierta aquí).
- No hay un quinto escenario "mixto" (varios endpoints combinados simulando un
  usuario navegando) — cada script prueba un endpoint aislado.
