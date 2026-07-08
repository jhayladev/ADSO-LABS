using AdsoLabs.Application.DTOs.Patrocinio;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Services;

public class PatrocinioService(AdsoDbContext db) : IPatrocinioService
{
    public async Task<int> AgregarPatrocinioAsync(AgregarPatrocinioCommand cmd)
    {
        // RN-PATROC-01: un aprendiz solo puede tener un patrocinio activo por ficha
        var yaExiste = await db.Patrocinio
            .AnyAsync(p => p.IdAprendiz == cmd.IdAprendiz
                        && p.IdFicha    == cmd.IdFicha
                        && p.Activo);

        if (yaExiste)
            throw new InvalidOperationException(
                "El aprendiz ya tiene un patrocinio activo en esta ficha. Desactívelo antes de crear uno nuevo.");

        if (cmd.Etapa is not ("Lectiva" or "Productiva"))
            throw new ArgumentException("Etapa inválida. Debe ser 'Lectiva' o 'Productiva'.");

        var patrocinio = new Patrocinio
        {
            IdAprendiz       = cmd.IdAprendiz,
            IdFicha          = cmd.IdFicha,
            Etapa            = cmd.Etapa,
            Activo           = true,
            FechaInicioEtapa = cmd.FechaInicioEtapa,
            FechaFinEtapa    = cmd.FechaFinEtapa,
            HoraInicio       = cmd.HoraInicio,
            HoraFin          = cmd.HoraFin,
            NombreEmpresa    = cmd.NombreEmpresa,
            ContactoEmpresa  = cmd.ContactoEmpresa,
            FechaRegistro    = DateTime.UtcNow
        };

        db.Patrocinio.Add(patrocinio);
        await db.SaveChangesAsync();
        return patrocinio.IdPatrocinio;
    }

    public async Task ActualizarPatrocinioAsync(ActualizarPatrocinioCommand cmd)
    {
        var patrocinio = await db.Patrocinio
            .FirstOrDefaultAsync(p => p.IdPatrocinio == cmd.IdPatrocinio)
            ?? throw new KeyNotFoundException($"Patrocinio {cmd.IdPatrocinio} no encontrado.");

        if (cmd.Etapa is not ("Lectiva" or "Productiva"))
            throw new ArgumentException("Etapa inválida. Debe ser 'Lectiva' o 'Productiva'.");

        patrocinio.Etapa            = cmd.Etapa;
        patrocinio.Activo           = cmd.Activo;
        patrocinio.FechaInicioEtapa = cmd.FechaInicioEtapa;
        patrocinio.FechaFinEtapa    = cmd.FechaFinEtapa;
        patrocinio.HoraInicio       = cmd.HoraInicio;
        patrocinio.HoraFin          = cmd.HoraFin;
        patrocinio.NombreEmpresa    = cmd.NombreEmpresa;
        patrocinio.ContactoEmpresa  = cmd.ContactoEmpresa;

        await db.SaveChangesAsync();
    }

    public async Task EliminarPatrocinioAsync(int idPatrocinio)
    {
        var patrocinio = await db.Patrocinio
            .FirstOrDefaultAsync(p => p.IdPatrocinio == idPatrocinio)
            ?? throw new KeyNotFoundException($"Patrocinio {idPatrocinio} no encontrado.");

        // Soft delete: desactivar en lugar de eliminar físicamente
        patrocinio.Activo = false;
        await db.SaveChangesAsync();
    }

    public async Task RegistrarAsistenciasAsync(RegistrarAsistenciasCommand cmd)
    {
        var existePatrocinio = await db.Patrocinio
            .AnyAsync(p => p.IdPatrocinio == cmd.IdPatrocinio);

        if (!existePatrocinio)
            throw new KeyNotFoundException($"Patrocinio {cmd.IdPatrocinio} no encontrado.");

        foreach (var item in cmd.Asistencias)
        {
            if (item.Estado is not ("Presente" or "Ausente" or "Justificado"))
                throw new ArgumentException($"Estado de asistencia inválido: '{item.Estado}'.");

            var existente = await db.AsistenciaPatrocinio
                .FirstOrDefaultAsync(a => a.IdPatrocinio == cmd.IdPatrocinio
                                       && a.Fecha        == item.Fecha);

            if (existente is not null)
            {
                existente.Estado      = item.Estado;
                existente.Observacion = item.Observacion;
            }
            else
            {
                db.AsistenciaPatrocinio.Add(new AsistenciaPatrocinio
                {
                    IdPatrocinio  = cmd.IdPatrocinio,
                    Fecha         = item.Fecha,
                    Estado        = item.Estado,
                    Observacion   = item.Observacion,
                    FechaRegistro = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task AsignarHorarioAsync(AsignarHorarioCommand cmd)
    {
        var patrocinio = await db.Patrocinio
            .FirstOrDefaultAsync(p => p.IdPatrocinio == cmd.IdPatrocinio)
            ?? throw new KeyNotFoundException($"Patrocinio {cmd.IdPatrocinio} no encontrado.");

        patrocinio.HoraInicio = cmd.HoraInicio;
        patrocinio.HoraFin    = cmd.HoraFin;

        await db.SaveChangesAsync();
    }

    public async Task EliminarHorarioAsync(int idPatrocinio)
    {
        var patrocinio = await db.Patrocinio
            .FirstOrDefaultAsync(p => p.IdPatrocinio == idPatrocinio)
            ?? throw new KeyNotFoundException($"Patrocinio {idPatrocinio} no encontrado.");

        patrocinio.HoraInicio = null;
        patrocinio.HoraFin    = null;

        await db.SaveChangesAsync();
    }
}
