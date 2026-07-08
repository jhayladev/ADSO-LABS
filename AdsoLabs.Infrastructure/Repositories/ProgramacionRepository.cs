using AdsoLabs.Application.Common.Helpers;
using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Fichas;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class ProgramacionRepository(AdsoDbContext context) : IProgramacionRepository
{
    public async Task<MensajeEstadoDTO> ConfigurarFichaCompetenciaAsync(ConfigurarFichaCompetenciaDTO dto)
    {
        if (dto.FechaFin < dto.FechaInicio)
            return Error("La fecha de fin no puede ser anterior a la fecha de inicio.");

        if (dto.TotalHoras <= 0)
            return Error("El total de horas debe ser mayor a 0.");

        // Resolver idFicha y obtener rango de la ficha para validación
        var fichaInfo = await context.Ficha
            .Where(f => f.NumeroFicha == dto.NumeroFicha)
            .Select(f => new { f.IdFicha, f.FechaInicio, f.FechaFin })
            .FirstOrDefaultAsync();

        if (fichaInfo is null)
            return Error("No se encontró la ficha indicada.");

        // Validar que el rango de la competencia esté dentro del rango de la ficha
        if (dto.FechaInicio < fichaInfo.FechaInicio)
            return Error(
                $"La fecha de inicio de la competencia ({dto.FechaInicio:dd/MM/yyyy}) " +
                $"no puede ser anterior al inicio de la ficha ({fichaInfo.FechaInicio:dd/MM/yyyy}).");

        if (fichaInfo.FechaFin.HasValue && dto.FechaFin > fichaInfo.FechaFin.Value)
            return Error(
                $"La fecha de fin de la competencia ({dto.FechaFin:dd/MM/yyyy}) " +
                $"no puede superar la fecha de fin de la ficha ({fichaInfo.FechaFin.Value:dd/MM/yyyy}).");

        var fc = await context.FichaCompetencia
            .FirstOrDefaultAsync(x => x.IdFicha == fichaInfo.IdFicha && x.IdCompetencia == dto.IdCompetencia);

        if (fc is null)
        {
            fc = new FichaCompetencia
            {
                IdFicha       = fichaInfo.IdFicha,
                IdCompetencia = dto.IdCompetencia,
                Estado        = "Pendiente"
            };
            context.FichaCompetencia.Add(fc);
        }

        fc.FechaInicio = dto.FechaInicio;
        fc.FechaFin    = dto.FechaFin;
        fc.TotalHoras  = dto.TotalHoras;

        await context.SaveChangesAsync();
        return new MensajeEstadoDTO { esExitoso = true, Mensaje = "Competencia configurada correctamente." };
    }

    public async Task<MensajeEstadoDTO> ProgramarResultadoAsync(ProgramarResultadoDTO dto)
    {
        // ── Validaciones básicas ─────────────────────────────────────────────
        if (dto.HorasProgramadas <= 0)
            return Error("Las horas programadas deben ser mayores a 0.");

        if (dto.HoraFin <= dto.HoraInicio)
            return Error("La hora de fin debe ser mayor a la hora de inicio.");

        if (dto.FechaFin < dto.FechaInicio)
            return Error("La fecha de fin no puede ser anterior a la fecha de inicio.");

        // ── Regla: máximo 1 trimestre (3 meses) por resultado ───────────────
        if (dto.FechaFin > dto.FechaInicio.AddMonths(3))
            return Error(
                $"La programación de un resultado no puede superar los 3 meses (1 trimestre). " +
                $"La fecha máxima permitida desde {dto.FechaInicio:dd/MM/yyyy} es " +
                $"{dto.FechaInicio.AddMonths(3):dd/MM/yyyy}.");

        // ── Regla: las fechas no pueden caer en domingo ni festivo ───────────
        var tipoInicio = FestivosColombiaHelper.DescripcionDomingoOFestivo(dto.FechaInicio);
        if (tipoInicio is not null)
            return Error(
                $"La fecha de inicio ({dto.FechaInicio:dd/MM/yyyy}) cae en {tipoInicio}. " +
                "No se puede programar en días festivos ni domingos.");

        var tipoFin = FestivosColombiaHelper.DescripcionDomingoOFestivo(dto.FechaFin);
        if (tipoFin is not null)
            return Error(
                $"La fecha de fin ({dto.FechaFin:dd/MM/yyyy}) cae en {tipoFin}. " +
                "No se puede programar en días festivos ni domingos.");

        // Validar que el rango fechas + horario diario alcanza para cubrir las horas programadas
        double horasDiarias = (dto.HoraFin - dto.HoraInicio).TotalHours;
        int    diasTotales  = (dto.FechaFin.DayNumber - dto.FechaInicio.DayNumber) + 1;
        double horasMaximas = horasDiarias * diasTotales;

        if (dto.HorasProgramadas > horasMaximas)
            return Error(
                $"Las {dto.HorasProgramadas} horas programadas no caben en el rango de fechas. " +
                $"Con {horasDiarias:0.#} h/día durante {diasTotales} día(s) solo es posible " +
                $"cubrir hasta {horasMaximas:0.#} horas.");

        // ── Validar fechas contra el rango de la ficha ───────────────────────
        var fichaInfo = await context.Ficha
            .Where(f => f.NumeroFicha == dto.NumeroFicha)
            .Select(f => new { f.IdFicha, f.FechaInicio, f.FechaFin })
            .FirstOrDefaultAsync();

        if (fichaInfo is null)
            return Error("No se encontró la ficha indicada.");

        if (dto.FechaInicio < fichaInfo.FechaInicio)
            return Error(
                $"La fecha de inicio del resultado ({dto.FechaInicio:dd/MM/yyyy}) " +
                $"no puede ser anterior al inicio de la ficha ({fichaInfo.FechaInicio:dd/MM/yyyy}).");

        if (fichaInfo.FechaFin.HasValue && dto.FechaFin > fichaInfo.FechaFin.Value)
            return Error(
                $"La fecha de fin del resultado ({dto.FechaFin:dd/MM/yyyy}) " +
                $"no puede superar la fecha de fin de la ficha ({fichaInfo.FechaFin.Value:dd/MM/yyyy}).");

        // ── Resolver IdFichaCompetencia ──────────────────────────────────────
        var idFichaCompetencia = await context.FichaCompetencia
            .Where(fc =>
                fc.Ficha.NumeroFicha == dto.NumeroFicha &&
                fc.IdCompetencia     == dto.IdCompetencia)
            .Select(fc => (int?)fc.IdFichaCompetencia)
            .FirstOrDefaultAsync();

        if (idFichaCompetencia is null)
            return Error("La competencia no está asignada a la ficha indicada.");

        int idFC = idFichaCompetencia.Value;

        // ── Cargar estado y ventana de fechas de la FichaCompetencia ────────
        var fcDatos = await context.FichaCompetencia
            .Where(fc => fc.IdFichaCompetencia == idFC)
            .Select(fc => new
            {
                fc.Estado,
                fc.FechaInicio,
                fc.FechaFin,
                // TotalHoras es la fuente de verdad; HorasAsignadas del catálogo es fallback
                HorasDisponibles = fc.TotalHoras ?? fc.Competencia.HorasAsignadas
            })
            .FirstAsync();

        if (fcDatos.Estado == "Vista")
            return Error("La competencia ya está en estado Vista y no admite nuevas programaciones.");

        // Validar que el resultado cabe dentro de la ventana de la competencia
        if (fcDatos.FechaFin.HasValue && dto.FechaFin > fcDatos.FechaFin.Value)
            return Error(
                $"La fecha de fin del resultado ({dto.FechaFin:dd/MM/yyyy}) supera la fecha de fin " +
                $"de la competencia ({fcDatos.FechaFin.Value:dd/MM/yyyy}).");

        if (fcDatos.FechaInicio.HasValue && dto.FechaInicio < fcDatos.FechaInicio.Value)
            return Error(
                $"La fecha de inicio del resultado ({dto.FechaInicio:dd/MM/yyyy}) es anterior a la fecha de inicio " +
                $"de la competencia ({fcDatos.FechaInicio.Value:dd/MM/yyyy}).");

        // ── Cargar el registro existente (si hay) ────────────────────────────
        var registro = await context.FichaCompetenciaResultado
            .FirstOrDefaultAsync(fcr =>
                fcr.IdFichaCompetencia == idFC &&
                fcr.IdResultado        == dto.IdResultado);

        // ID del registro actual — usado para excluirlo de los checks de conflicto
        // cuando se trata de una actualización (no de un registro nuevo).
        int idRegistro = registro?.IdFichaCompetenciaResultado ?? 0;

        // Un resultado Completado tiene juicio APROBADO para todos los aprendices.
        // La importación de juicios evaluativos es quien lo completó, y se asume
        // que la programación ya existía antes de importar (Tarea 1 lo garantiza).
        // No se permite reprogramar un resultado completado.
        if (registro?.Estado == "Completado")
            return Error(
                "Este resultado ya está completado: todos los aprendices activos tienen juicio APROBADO. " +
                "No es posible reprogramarlo.");

        // ── Validación de conflictos ─────────────────────────────────────────
        // 1) Conflicto dentro de la misma FICHA (cualquier competencia):
        //    los aprendices no pueden estar en dos resultados al mismo tiempo.
        //    Condición: rango de fechas solapado Y rango de horas solapado.
        var solapamientoFicha = await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.IdFichaCompetenciaResultado != idRegistro           &&
                fcr.FichaCompetencia.IdFicha    == fichaInfo.IdFicha    &&
                fcr.Estado                      == "Programado"         &&
                // solapamiento de fechas
                fcr.FechaInicio <= dto.FechaFin                         &&
                fcr.FechaFin    >= dto.FechaInicio                      &&
                // solapamiento de horario diario
                fcr.HoraInicio  <  dto.HoraFin                          &&
                fcr.HoraFin     >  dto.HoraInicio)
            .Select(fcr => new
            {
                NombreResultado   = fcr.Resultado.Nombre,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre
            })
            .FirstOrDefaultAsync();

        if (solapamientoFicha is not null)
            return Error(
                $"Conflicto de horario con el resultado «{solapamientoFicha.NombreResultado}» " +
                $"de la competencia «{solapamientoFicha.NombreCompetencia}». " +
                "Los aprendices no pueden estar en dos resultados al mismo tiempo.");

        // 2) Conflicto del instructor en CUALQUIER ficha:
        //    un instructor no puede dictar dos resultados simultáneamente.
        var conflictoInstructor = await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.IdFichaCompetenciaResultado != idRegistro   &&
                fcr.IdInstructor                == dto.IdInstructor &&
                fcr.Estado                      == "Programado"  &&
                fcr.FechaInicio <= dto.FechaFin                  &&
                fcr.FechaFin    >= dto.FechaInicio               &&
                fcr.HoraInicio  <  dto.HoraFin                   &&
                fcr.HoraFin     >  dto.HoraInicio)
            .Select(fcr => new
            {
                NombreResultado   = fcr.Resultado.Nombre,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                NumeroFicha       = fcr.FichaCompetencia.Ficha.NumeroFicha
            })
            .FirstOrDefaultAsync();

        if (conflictoInstructor is not null)
            return Error(
                $"El instructor ya tiene programado el resultado «{conflictoInstructor.NombreResultado}» " +
                $"de la competencia «{conflictoInstructor.NombreCompetencia}» " +
                $"(ficha {conflictoInstructor.NumeroFicha}) en ese mismo horario.");

        // ── Validar techo de horas ───────────────────────────────────────────
        var horasYaProgramadas = await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.IdFichaCompetencia == idFC            &&
                fcr.IdResultado        != dto.IdResultado &&
                fcr.Estado             == "Programado")
            .SumAsync(fcr => fcr.HorasProgramadas ?? 0);

        int horasTotalesUsadas = horasYaProgramadas + dto.HorasProgramadas;
        if (horasTotalesUsadas > fcDatos.HorasDisponibles)
            return Error(
                $"Las horas programadas superan el total permitido de la competencia " +
                $"({fcDatos.HorasDisponibles} horas). " +
                $"Ya programadas: {horasYaProgramadas} h · " +
                $"Disponibles: {fcDatos.HorasDisponibles - horasYaProgramadas} h.");

        // ── Crear o actualizar el registro ───────────────────────────────────
        if (registro is null)
        {
            registro = new FichaCompetenciaResultado
            {
                IdFichaCompetencia = idFC,
                IdResultado        = dto.IdResultado
            };
            context.FichaCompetenciaResultado.Add(registro);
        }

        registro.IdInstructor     = dto.IdInstructor;
        registro.FechaInicio      = dto.FechaInicio;
        registro.FechaFin         = dto.FechaFin;
        registro.HoraInicio       = dto.HoraInicio;
        registro.HoraFin          = dto.HoraFin;
        registro.HorasProgramadas = dto.HorasProgramadas;
        registro.Estado           = "Programado";

        await context.SaveChangesAsync();

        // Recalcular estado y horas ejecutadas de la FichaCompetencia
        await ActualizarFichaCompetenciaAsync(idFC);
        await context.SaveChangesAsync();

        return new MensajeEstadoDTO { esExitoso = true, Mensaje = "Resultado programado exitosamente." };
    }

    // ─── helpers ────────────────────────────────────────────────────────────────

    private async Task ActualizarFichaCompetenciaAsync(int idFichaCompetencia)
    {
        var fc = await context.FichaCompetencia
            .FirstAsync(f => f.IdFichaCompetencia == idFichaCompetencia);

        // Traer todos los resultados con datos de programación (Programado o Completado con horas)
        var conProgramacion = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdFichaCompetencia == idFichaCompetencia &&
                          (fcr.Estado == "Programado" || fcr.Estado == "Completado") &&
                          fcr.HorasProgramadas != null)
            .ToListAsync();

        int cantProgramados = conProgramacion.Count(r => r.Estado == "Programado");

        // "Vista" es exclusivo de la importación SOFIA Plus (requiere juicios APROBADO).
        // El flujo de programación solo puede llevar la competencia hasta "Programada".
        fc.Estado = cantProgramados == 0 ? "Pendiente" : "Programada";

        // FechaInicio y FechaFin son campos configurados manualmente; no se sobreescriben.

        // Horas ejecutadas = suma de horas de resultados Completados únicamente.
        fc.HorasEjecutadas = conProgramacion
            .Where(p => p.Estado == "Completado")
            .Sum(p => p.HorasProgramadas ?? 0);
    }

    public async Task<MensajeEstadoDTO> EliminarProgramacionResultadoAsync(int idFichaCompetenciaResultado)
    {
        var registro = await context.FichaCompetenciaResultado
            .FirstOrDefaultAsync(fcr => fcr.IdFichaCompetenciaResultado == idFichaCompetenciaResultado);

        if (registro is null)
            return Error("No se encontró la programación indicada.");

        if (registro.Estado != "Programado")
            return Error("Solo se puede eliminar la programación de resultados en estado Programado.");

        // Verificar que no haya sesiones ejecutadas para este resultado
        var tieneSesiones = await context.Sesion
            .AnyAsync(s => s.IdFichaCompetenciaResultado == idFichaCompetenciaResultado);

        if (tieneSesiones)
            return Error("No se puede eliminar la programación porque ya existen sesiones ejecutadas para este resultado.");

        int idFC = registro.IdFichaCompetencia;

        // Resetear a Pendiente y limpiar campos de programación
        registro.Estado           = "Pendiente";
        registro.IdInstructor     = null;
        registro.FechaInicio      = null;
        registro.FechaFin         = null;
        registro.HoraInicio       = null;
        registro.HoraFin          = null;
        registro.HorasProgramadas = null;

        await context.SaveChangesAsync();

        // Recalcular estado de la FichaCompetencia
        await ActualizarFichaCompetenciaAsync(idFC);
        await context.SaveChangesAsync();

        return new MensajeEstadoDTO { esExitoso = true, Mensaje = "Programación eliminada correctamente." };
    }

    private static MensajeEstadoDTO Error(string mensaje)
        => new() { esExitoso = false, Mensaje = mensaje };
}
