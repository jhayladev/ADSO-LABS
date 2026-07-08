using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Importacion;
using AdsoLabs.Application.Importacion;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using AdsoLabs.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.API.Controllers
{
    [Route("api/")]
    [ApiController]
    // ImportarReporte (juicios SOFIA) tambien lo usa Fichas.razor (Admin+Instructor).
    // El resto de acciones (historial, plantillas, importar-fichas/aprendices) son
    // exclusivas de Importaciones.razor y se restringen a Administrador mas abajo.
    [Authorize(Roles = "Administrador,Instructor")]
    public class ImportacionController(
        AdsoDbContext            db,
        ImportadorReporteJuicios importador,
        ImportadorFichas         importadorFichas,
        ImportadorAprendices     importadorAprendices,
        ImportacionAppService    importacionAppService)
        : ControllerBase
    {
        // ── POST /api/importar-reporte ────────────────────────────────────────
        [HttpPost("importar-reporte")]
        public async Task<IActionResult> ImportarReporte(IFormFile archivo)
        {
            var idUsuario = this.ObtenerIdUsuarioAutenticado();
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { Errores = new[] { "No se recibió ningún archivo." } });

            string? ruta = null;
            try
            {
                ruta = Path.Combine(Path.GetTempPath(),
                    Guid.NewGuid() + Path.GetExtension(archivo.FileName));
                await using (var fs = System.IO.File.Create(ruta))
                    await archivo.CopyToAsync(fs);

                var tipoExcel = await db.TipoArchivo
                    .FirstOrDefaultAsync(t => t.Nombre == "Importacion Masiva");
                if (tipoExcel == null)
                    return BadRequest(new
                    {
                        Errores = new[] { "No existe el tipo de archivo 'Importacion Masiva' en la base de datos." }
                    });

                var archivoEntity = new Archivo
                {
                    IdTipoArchivo    = tipoExcel.IdTipoArchivo,
                    IdUsuarioSubio   = idUsuario,
                    NombreOriginal   = archivo.FileName,
                    NombreAlmacenado = archivo.FileName,
                    ExtensionArchivo = Path.GetExtension(archivo.FileName),
                    MimeType         = archivo.ContentType,
                    TamanioBytes     = archivo.Length,
                    RutaStorage      = ruta
                };
                db.Archivo.Add(archivoEntity);
                await db.SaveChangesAsync();

                var resultado = await importador.ImportarAsync(
                    rutaExcel:          ruta,
                    idUsuarioAdmin:     idUsuario,
                    idArchivoRegistro:  archivoEntity.IdArchivo,
                    idInstructorDefault: idUsuario);

                return Ok(new
                {
                    resultado.IdImportacion,
                    resultado.TotalFilas,
                    resultado.FilasOk,
                    resultado.FilasError,
                    resultado.AprendicesCreados,
                    resultado.AprendicesActualizados,
                    resultado.JuiciosInsertados,
                    resultado.JuiciosActualizados,
                    FichasAfectadas = resultado.FichasAfectadas.ToList(),
                    resultado.Errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Errores = new[] { $"Error durante la importación: {ex.Message}" } });
            }
            finally
            {
                if (ruta != null && System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
            }
        }

        // ── GET /api/importaciones ────────────────────────────────────────────
        [HttpGet("importaciones")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerHistorial()
            => Ok(await importacionAppService.ObtenerHistorialAsync());

        // ── GET /api/importaciones/{id}/errores ───────────────────────────────
        [HttpGet("importaciones/{id:int}/errores")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerErrores(int id)
            => Ok(await importacionAppService.ObtenerErroresAsync(id));

        // ── GET /api/plantilla-fichas ─────────────────────────────────────────
        /// <summary>
        /// Devuelve el archivo .xlsx de plantilla oficial para importación de fichas.
        /// El archivo se genera en memoria al vuelo (sin disco, sin caché) a partir
        /// de <see cref="PlantillaFichasGenerator"/> — no requiere dependencias externas.
        /// </summary>
        [HttpGet("plantilla-fichas")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DescargarPlantillaFichas()
        {
            var bytes = PlantillaFichasGenerator.Generar();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PlantillaFichas.xlsx");
        }

        // ── POST /api/importar-fichas ─────────────────────────────────────────
        /// <summary>
        /// Importa fichas y competencias desde la plantilla Excel oficial.
        /// Acepta multipart/form-data con el campo "archivo" (.xlsx).
        ///
        /// Comportamiento:
        ///   • Crea fichas nuevas o actualiza las existentes (Nombre, Fechas).
        ///   • Vincula competencias a la ficha si el vínculo no existe.
        ///   • Idempotente: importar el mismo archivo dos veces no duplica datos.
        ///   • Devuelve un resumen con contadores y lista de errores por fila.
        /// </summary>
        [HttpPost("importar-fichas")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ImportarFichas(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { Errores = new[] { "No se recibió ningún archivo." } });

            if (!archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Errores = new[] { "Solo se acepta el archivo .xlsx de la plantilla oficial de fichas." } });

            string? rutaTemporal = null;
            try
            {
                // Guardar en disco temporal para que ExcelDataReader pueda abrirlo
                rutaTemporal = Path.Combine(
                    Path.GetTempPath(),
                    $"{Guid.NewGuid()}.xlsx");

                await using (var fs = System.IO.File.Create(rutaTemporal))
                    await archivo.CopyToAsync(fs);

                var resultado = await importadorFichas.ImportarAsync(rutaTemporal);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Errores = new[] { $"Error durante la importación: {ex.Message}" }
                });
            }
            finally
            {
                // Limpiar el archivo temporal independientemente del resultado
                if (rutaTemporal != null && System.IO.File.Exists(rutaTemporal))
                    System.IO.File.Delete(rutaTemporal);
            }
        }

        // ── GET /api/plantilla-aprendices ─────────────────────────────────────
        /// <summary>
        /// Devuelve el archivo .xlsx de plantilla oficial para importación de aprendices.
        /// El archivo se genera en memoria al vuelo (sin disco, sin caché) a partir
        /// de <see cref="PlantillaAprendizGenerator"/> — no requiere dependencias externas.
        /// </summary>
        [HttpGet("plantilla-aprendices")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DescargarPlantillaAprendices()
        {
            var bytes = PlantillaAprendizGenerator.Generar();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PlantillaAprendices.xlsx");
        }

        // ── POST /api/importar-aprendices/{numeroFicha} ───────────────────────
        /// <summary>
        /// Importa aprendices desde la plantilla Excel oficial y los vincula
        /// a la ficha indicada en la ruta.
        /// Acepta multipart/form-data con el campo "archivo" (.xlsx).
        ///
        /// El número de ficha viene del parámetro de ruta, NO del Excel.
        /// Esto evita errores de digitación y acopla el archivo al flujo correcto.
        ///
        /// Comportamiento:
        ///   • Crea Persona + AprendizPerfil si el documento no existe.
        ///   • Actualiza Nombres y Apellidos si la Persona ya existe.
        ///   • Vincula el aprendiz a la ficha con el estado del Excel.
        ///   • Aplica regla de traslado automático (CLAUDE.md §7.9).
        ///   • Idempotente: importar el mismo archivo dos veces no duplica datos.
        ///   • Devuelve un resumen con contadores y lista de errores por fila.
        /// </summary>
        [HttpPost("importar-aprendices/{numeroFicha}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ImportarAprendices(
            string numeroFicha,
            IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { Errores = new[] { "No se recibió ningún archivo." } });

            bool esPlantilla = archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
            bool esSofia     = archivo.FileName.EndsWith(".xls",  StringComparison.OrdinalIgnoreCase)
                               && !esPlantilla;

            if (!esPlantilla && !esSofia)
                return BadRequest(new
                {
                    Errores = new[]
                    {
                        "Formato no aceptado. Use la plantilla oficial de aprendices (.xlsx) " +
                        "o el Reporte de Juicios Evaluativos exportado desde SOFIA Plus (.xls)."
                    }
                });

            if (string.IsNullOrWhiteSpace(numeroFicha))
                return BadRequest(new { Errores = new[] { "El número de ficha es obligatorio." } });

            string? rutaTemporal = null;
            try
            {
                // Preservar la extensión original: el importer la usa para detectar el formato
                string ext = Path.GetExtension(archivo.FileName);
                rutaTemporal = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext}");

                await using (var fs = System.IO.File.Create(rutaTemporal))
                    await archivo.CopyToAsync(fs);

                var resultado = await importadorAprendices.ImportarAsync(rutaTemporal, numeroFicha);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Errores = new[] { $"Error durante la importación: {ex.Message}" }
                });
            }
            finally
            {
                // Limpiar el archivo temporal independientemente del resultado
                if (rutaTemporal != null && System.IO.File.Exists(rutaTemporal))
                    System.IO.File.Delete(rutaTemporal);
            }
        }
    }
}
