using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.Common.Helpers;
using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Asistencia;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class AsistenciaRepository(AdsoDbContext context) : IAsistenciaRepository
{
    // ── Crear sesión ──────────────────────────────────────────────────────────
    public async Task<MensajeEstadoDTO> CrearSesionAsync(CrearSesionDTO dto)
    {
        if (dto.HoraFin <= dto.HoraInicio)
            return Error("La hora de fin debe ser mayor a la hora de inicio.");

        // Domingos y festivos colombianos no están permitidos
        var tipoDia = FestivosColombiaHelper.DescripcionDomingoOFestivo(dto.Fecha);
        if (tipoDia is not null)
            return Error($"No se pueden crear sesiones en {tipoDia}. Selecciona otro día.");

        // Cargar resultado con su rango y datos del instructor
        var fcr = await context.FichaCompetenciaResultado
            .Where(r => r.IdFichaCompetenciaResultado == dto.IdFichaCompetenciaResultado)
            .Select(r => new
            {
                r.IdFichaCompetencia,
                r.IdInstructor,
                r.FechaInicio,
                r.FechaFin,
                r.Estado
            })
            .FirstOrDefaultAsync();

        if (fcr is null)
            return Error("No se encontró el resultado de aprendizaje indicado.");

        if (fcr.Estado == "Pendiente")
            return Error("Este resultado aún no está programado.");

        if (fcr.FechaInicio is null || fcr.FechaFin is null)
            return Error("El resultado no tiene fechas de programación definidas.");

        if (dto.Fecha < fcr.FechaInicio.Value || dto.Fecha > fcr.FechaFin.Value)
            return Error(
                $"La fecha {dto.Fecha:dd/MM/yyyy} está fuera del rango de programación " +
                $"({fcr.FechaInicio.Value:dd/MM/yyyy} – {fcr.FechaFin.Value:dd/MM/yyyy}).");

        if (fcr.IdInstructor is null)
            return Error("El resultado no tiene instructor asignado.");

        // Evitar sesión duplicada para la misma fecha en la misma ficha-competencia
        var existe = await context.Sesion
            .AnyAsync(s => s.IdFichaCompetencia == fcr.IdFichaCompetencia && s.Fecha == dto.Fecha);

        if (existe)
            return Error($"Ya existe una sesión registrada para el {dto.Fecha:dd/MM/yyyy}.");

        var sesion = new Sesion
        {
            IdFichaCompetencia          = fcr.IdFichaCompetencia,
            IdFichaCompetenciaResultado = dto.IdFichaCompetenciaResultado,
            IdInstructor                = fcr.IdInstructor.Value,
            Fecha                       = dto.Fecha,
            HoraInicio                  = dto.HoraInicio,
            HoraFin                     = dto.HoraFin,
            Estado                      = EstadosSesion.Abierta
        };

        context.Sesion.Add(sesion);
        await context.SaveChangesAsync();

        return new MensajeEstadoDTO
        {
            esExitoso = true,
            Mensaje   = "Sesión creada correctamente.",
            IdCreado  = sesion.IdSesion
        };
    }

    // ── Guardar asistencia ────────────────────────────────────────────────────
    public async Task<MensajeEstadoDTO> GuardarAsistenciaAsync(GuardarAsistenciaDTO dto)
    {
        if (dto.Items.Count == 0)
            return Error("Debe incluir al menos un aprendiz.");

        // Validar estados
        var estadosValidos = new HashSet<string>
        {
            EstadosAsistencia.Presente,
            EstadosAsistencia.Ausente,
            EstadosAsistencia.Justificado,
            EstadosAsistencia.Tarde
        };
        var invalido = dto.Items.FirstOrDefault(i => !estadosValidos.Contains(i.Estado));
        if (invalido is not null)
            return Error($"Estado «{invalido.Estado}» no válido. Use: Presente, Ausente, Justificado o Tarde.");

        // Verificar que la sesión existe y está abierta
        var sesion = await context.Sesion
            .FirstOrDefaultAsync(s => s.IdSesion == dto.IdSesion);

        if (sesion is null)
            return Error("No se encontró la sesión indicada.");

        if (sesion.Estado == EstadosSesion.Cancelada)
            return Error("No se puede registrar asistencia en una sesión cancelada.");

        // Upsert de asistencia
        var idAprendices = dto.Items.Select(i => i.IdAprendiz).ToList();
        var existentes = await context.Asistencia
            .Where(a => a.IdSesion == dto.IdSesion && idAprendices.Contains(a.IdAprendiz))
            .ToListAsync();

        var mapaExistentes = existentes.ToDictionary(a => a.IdAprendiz);

        foreach (var item in dto.Items)
        {
            if (mapaExistentes.TryGetValue(item.IdAprendiz, out var registro))
            {
                registro.Estado             = item.Estado;
                registro.MotivoInasistencia = item.MotivoInasistencia;
            }
            else
            {
                context.Asistencia.Add(new Asistencia
                {
                    IdSesion           = dto.IdSesion,
                    IdAprendiz         = item.IdAprendiz,
                    Estado             = item.Estado,
                    MotivoInasistencia = item.MotivoInasistencia
                });
            }
        }

        // Marcar sesión como Finalizada si todos los aprendices están registrados
        if (sesion.Estado == EstadosSesion.Abierta)
            sesion.Estado = EstadosSesion.Finalizada;

        await context.SaveChangesAsync();
        return new MensajeEstadoDTO { esExitoso = true, Mensaje = "Asistencia guardada correctamente." };
    }

    private static MensajeEstadoDTO Error(string mensaje)
        => new() { esExitoso = false, Mensaje = mensaje };
}
