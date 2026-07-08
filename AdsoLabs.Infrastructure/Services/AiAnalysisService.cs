using System.Net;
using System.Text;
using System.Text.Json;
using AdsoLabs.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace AdsoLabs.Infrastructure.Services;

public class AiAnalysisService(HttpClient httpClient, IConfiguration config) : IAiAnalysisService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private string ApiKey => config["Anthropic:ApiKey"]
        ?? throw new InvalidOperationException("Falta configurar Anthropic:ApiKey (ver appsettings.Development.json).");
    private const string Model = "claude-haiku-4-5-20251001";
    private const string Endpoint = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    public async Task<CompatibilidadIAResult> AnalizarCompatibilidadAsync(InstructorIAContext ctx, CancellationToken ct = default)
    {
        var systemPrompt = @"Eres un asistente especializado en gestión académica del SENA Colombia.
Analiza la compatibilidad de un instructor con fichas de formación disponibles.
Las fichas ya vienen filtradas para respetar el orden curricular del proyecto formativo
(Inducción → Análisis → Planeación → Ejecución → Evaluación): no sugieras razonamientos que
contradigan la Fase formativa indicada para cada competencia (ej. no trates una competencia de
Ejecución como si la ficha ya la necesitara si su fase formativa aún es temprana).
Responde ÚNICAMENTE con JSON válido sin explicaciones adicionales, siguiendo exactamente el esquema solicitado.";

        var userPrompt = $@"Instructor: {ctx.NombreInstructor} | Especialidad: {ctx.Especialidad}
Competencias que ha impartido históricamente:
{FormatearCompetencias(ctx.CompetenciasHistoricas)}

Fichas disponibles para asignación:
{FormatearFichasDisponibles(ctx.FichasDisponibles)}

Devuelve JSON con este esquema exacto:
{{
  ""porcentajeGeneral"": 85,
  ""resumenIA"": ""Texto breve de máximo 2 oraciones sobre el perfil del instructor"",
  ""fortalezas"": [""Fortaleza 1"", ""Fortaleza 2"", ""Fortaleza 3""],
  ""sugerencias"": [
    {{
      ""idFichaCompetencia"": 123,
      ""compatibilidad"": 90,
      ""razonamiento"": ""Por qué es compatible con esta ficha/competencia (1-2 oraciones)""
    }}
  ]
}}
Incluye sugerencias para TODAS las fichas disponibles, ordenadas de mayor a menor compatibilidad.";

        var json = await CallClaudeAsync(systemPrompt, userPrompt, ct, maxTokens: 2000);
        return ParseCompatibilidad(json, ctx);
    }

    public async Task<PrediccionAprendizajeIAResult> PredecirAprendizajeAsync(PrediccionCompetenciaContext ctx, CancellationToken ct = default)
    {
        var systemPrompt = @"Eres un experto pedagógico del SENA Colombia especializado en formación técnica en Análisis y Desarrollo de Software (ADSO).
Analiza una competencia específica que un instructor debe impartir y genera predicciones pedagógicas y recomendaciones concretas para sus aprendices.
Responde ÚNICAMENTE con JSON válido sin explicaciones adicionales.";

        var resultados = ctx.ResultadosAsignados.Count > 0
            ? string.Join("\n", ctx.ResultadosAsignados.Select((r, i) => $"  {i + 1}. {r}"))
            : "  (sin resultados registrados)";

        var descripcionCompetencia = string.IsNullOrWhiteSpace(ctx.DescripcionCompetencia)
            ? "(sin descripción oficial registrada)"
            : ctx.DescripcionCompetencia;

        var userPrompt = $@"Instructor: {ctx.NombreInstructor} | Especialidad: {ctx.Especialidad}
Competencia a impartir: {ctx.NombreCompetencia} (código: {ctx.CodigoCompetencia}) — Ficha: {ctx.NumeroFicha}
Descripción oficial de la competencia (Resultados de Aprendizaje según el catálogo SENA):
{descripcionCompetencia}

Resultados de aprendizaje asignados al instructor en esta ficha:
{resultados}

Genera predicciones y recomendaciones pedagógicas ESPECÍFICAS para esta competencia y sus resultados de aprendizaje.
Las temáticas deben estar directamente relacionadas con los resultados asignados.
La ruta de aprendizaje debe ser una secuencia lógica para enseñar esta competencia.
Los KPIs deben medir el progreso esperado en esta competencia.

Devuelve JSON con este esquema exacto:
{{
  ""tendenciaGeneral"": ""Frase concisa sobre la tendencia tecnológica de esta competencia"",
  ""descripcionTendencia"": ""Párrafo de 2-3 oraciones explicando por qué esta competencia es relevante y qué tendencias aplican"",
  ""tematicas"": [
    {{
      ""numero"": 1,
      ""icono"": ""bi-code-slash"",
      ""titulo"": ""Tema específico relacionado con los resultados"",
      ""descripcion"": ""Cómo este tema se conecta con los resultados de aprendizaje"",
      ""relevancia"": ""Alta""
    }}
  ],
  ""rutaAprendizaje"": [
    {{
      ""num"": 1,
      ""titulo"": ""Paso concreto para impartir la competencia"",
      ""descripcion"": ""Descripción metodológica del paso""
    }}
  ],
  ""kpis"": [
    {{
      ""etiqueta"": ""Indicador de esta competencia"",
      ""valor"": ""Valor estimado"",
      ""icono"": ""bi-bar-chart-fill"",
      ""color"": ""blue""
    }}
  ]
}}
Genera exactamente 4 tematicas, 4 pasos de ruta de aprendizaje y 4 kpis.
Iconos: usar clases Bootstrap Icons (bi-*). Colores permitidos: green, blue, amber, violet.
Relevancias: Alta, Media, Baja.";

        var json = await CallClaudeAsync(systemPrompt, userPrompt, ct, maxTokens: 2500);
        return ParsePrediccion(json);
    }

    private async Task<JsonDocument> CallClaudeAsync(string system, string user, CancellationToken ct, int maxTokens = 1500)
    {
        const int maxAttempts = 3;
        var key = ApiKey;
        Exception? lastEx = null;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var body = new
            {
                model = Model,
                max_tokens = maxTokens,
                temperature = 0.4,
                system,
                messages = new[]
                {
                    new { role = "user", content = user }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Add("x-api-key", key);
            req.Headers.Add("anthropic-version", AnthropicVersion);

            try
            {
                var resp = await httpClient.SendAsync(req, ct);

                // Rate limit o error de servidor → reintentar
                if (resp.StatusCode == HttpStatusCode.TooManyRequests || (int)resp.StatusCode >= 500)
                {
                    lastEx = new HttpRequestException($"Anthropic respondió {(int)resp.StatusCode} en el intento #{attempt + 1}.");
                    await Task.Delay(TimeSpan.FromSeconds(1 + attempt), ct);
                    continue;
                }

                resp.EnsureSuccessStatusCode();

                var raw = await resp.Content.ReadAsStringAsync(ct);
                var root = JsonDocument.Parse(raw);
                var content = root.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "{}";

                // Si Claude truncó la respuesta (max_tokens insuficiente) o devolvió JSON
                // inválido, se trata como error transitorio y se reintenta en vez de
                // propagar la excepción sin capturar (causaba un 500 no manejado).
                try
                {
                    return JsonDocument.Parse(ExtraerJson(content));
                }
                catch (JsonException ex)
                {
                    lastEx = ex;
                    continue;
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                // Timeout del HttpClient (no cancelación del usuario) → reintentar
                lastEx = new HttpRequestException($"Timeout en el intento #{attempt + 1}.");
                continue;
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts - 1)
            {
                // Error de red → reintentar
                lastEx = ex;
                continue;
            }
        }

        throw lastEx ?? new HttpRequestException("Anthropic no respondió correctamente tras varios intentos.");
    }

    // Claude a veces envuelve el JSON en un bloque de código Markdown (```json ... ```)
    // pese a que el prompt pide únicamente JSON — se extrae el bloque si aparece.
    private static string ExtraerJson(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith('{'))
        {
            var inicio = trimmed.IndexOf('{');
            var fin = trimmed.LastIndexOf('}');
            if (inicio >= 0 && fin > inicio)
                trimmed = trimmed[inicio..(fin + 1)];
        }
        return trimmed;
    }

    private static CompatibilidadIAResult ParseCompatibilidad(JsonDocument doc, InstructorIAContext ctx)
    {
        try
        {
            var root = doc.RootElement;
            var porcentaje = root.TryGetProperty("porcentajeGeneral", out var pj) ? pj.GetInt32() : 70;
            var resumen = root.TryGetProperty("resumenIA", out var rs) ? rs.GetString() ?? "" : "";
            var fortalezas = root.TryGetProperty("fortalezas", out var ft)
                ? ft.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : [];
            var sugerencias = root.TryGetProperty("sugerencias", out var sg)
                ? sg.EnumerateArray().Select(x => new SugerenciaIAItem(
                    x.TryGetProperty("idFichaCompetencia", out var id) ? id.GetInt32() : 0,
                    x.TryGetProperty("compatibilidad", out var c) ? c.GetInt32() : 50,
                    x.TryGetProperty("razonamiento", out var r) ? r.GetString() ?? "" : ""
                )).ToList()
                : [];

            return new CompatibilidadIAResult(porcentaje, resumen, fortalezas, sugerencias);
        }
        catch
        {
            return new CompatibilidadIAResult(70, "Análisis completado.", [], []);
        }
    }

    private static PrediccionAprendizajeIAResult ParsePrediccion(JsonDocument doc)
    {
        try
        {
            var root = doc.RootElement;
            var tendencia = root.TryGetProperty("tendenciaGeneral", out var t) ? t.GetString() ?? "" : "";
            var descripcion = root.TryGetProperty("descripcionTendencia", out var d) ? d.GetString() ?? "" : "";

            var tematicas = root.TryGetProperty("tematicas", out var tm)
                ? tm.EnumerateArray().Select((x, i) => new TematicaIAItem(
                    x.TryGetProperty("numero", out var n) ? n.GetInt32() : i + 1,
                    x.TryGetProperty("icono", out var ic) ? ic.GetString() ?? "bi-star" : "bi-star",
                    x.TryGetProperty("titulo", out var ti) ? ti.GetString() ?? "" : "",
                    x.TryGetProperty("descripcion", out var de) ? de.GetString() ?? "" : "",
                    x.TryGetProperty("relevancia", out var re) ? re.GetString() ?? "Media" : "Media"
                )).ToList()
                : [];

            var ruta = root.TryGetProperty("rutaAprendizaje", out var ru)
                ? ru.EnumerateArray().Select((x, i) => new PasoRutaIAItem(
                    x.TryGetProperty("num", out var nu) ? nu.GetInt32() : i + 1,
                    x.TryGetProperty("titulo", out var ti) ? ti.GetString() ?? "" : "",
                    x.TryGetProperty("descripcion", out var de) ? de.GetString() ?? "" : ""
                )).ToList()
                : [];

            var kpis = root.TryGetProperty("kpis", out var kp)
                ? kp.EnumerateArray().Select(x => new KpiIAItem(
                    x.TryGetProperty("etiqueta", out var et) ? et.GetString() ?? "" : "",
                    x.TryGetProperty("valor", out var va) ? va.GetString() ?? "" : "",
                    x.TryGetProperty("icono", out var ic) ? ic.GetString() ?? "bi-bar-chart" : "bi-bar-chart",
                    x.TryGetProperty("color", out var co) ? co.GetString() ?? "blue" : "blue"
                )).ToList()
                : [];

            return new PrediccionAprendizajeIAResult(tendencia, descripcion, tematicas, ruta, kpis);
        }
        catch
        {
            return new PrediccionAprendizajeIAResult("Sin datos suficientes", "No hay suficiente historial para generar una predicción.", [], [], []);
        }
    }

    private static string FormatearCompetencias(IReadOnlyList<CompetenciaContextItem> items)
        => items.Count == 0
            ? "(sin historial previo)"
            : string.Join("\n", items.Select(c => $"- {c.NombreCompetencia} (código: {c.CodigoCompetencia}, impartida {c.VecesImpartida} veces, {c.TotalAprendices} aprendices)"));

    private static string FormatearFichasDisponibles(IReadOnlyList<FichaCompetenciaContextItem> items)
        => items.Count == 0
            ? "(no hay fichas disponibles)"
            : string.Join("\n", items.Select(f => $"- ID:{f.IdFichaCompetencia} | {f.NombreCompetencia} ({f.CodigoCompetencia}) | Ficha: {f.NumeroFicha} | Jornada: {f.Jornada} | Fase formativa: {f.FaseFormativa ?? "Transversal"} | Progreso: {f.ProgresoFicha}%"));
}
