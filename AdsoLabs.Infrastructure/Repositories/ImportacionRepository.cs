using AdsoLabs.Application.DTOs.Importacion;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class ImportacionRepository(AdsoDbContext context) : IImportacionRepository
{
    public async Task<List<ImportacionResumenDTO>> ObtenerHistorialAsync()
        => await context.ImportacionArchivo
            .AsNoTracking()
            .OrderByDescending(i => i.FechaSubida)
            .Select(i => new ImportacionResumenDTO
            {
                IdImportacion  = i.IdImportacion,
                NombreArchivo  = i.Documento.NombreOriginal,
                NombreUsuario  = i.Usuario.Persona.Nombres + " " + i.Usuario.Persona.Apellidos,
                Estado         = i.EstadoProcesamiento,
                Observacion    = i.Observacion ?? string.Empty,
                TotalFilas     = i.TotalFilas,
                FilasOk        = i.FilasOk,
                FilasError     = i.FilasError,
                FechaSubida    = i.FechaSubida,
                FechaProcesado = i.FechaProcesado
            })
            .Take(50)
            .ToListAsync();

    public async Task<List<ImportacionErrorDTO>> ObtenerErroresAsync(int idImportacion)
        => await context.ImportacionError
            .AsNoTracking()
            .Where(e => e.IdImportacion == idImportacion)
            .OrderBy(e => e.NumeroFila)
            .Select(e => new ImportacionErrorDTO
            {
                NumeroFila  = e.NumeroFila,
                MotivoError = e.MotivoError,
                DatosFila   = e.DatosFila
            })
            .ToListAsync();
}
