using AdsoLabs.Application.DTOs.Perfil;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Services;

public class PerfilService(AdsoDbContext db) : IPerfilService
{
    public async Task<PerfilAprendizDTO?> ObtenerPerfilAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil
            .Include(a => a.Persona)
            .Include(a => a.Usuario)
            .Include(a => a.FichaAprendices).ThenInclude(fa => fa.Ficha)
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

        if (aprendiz is null) return null;

        var tipoDoc = await db.TipoDocumento
            .Where(t => t.IdTipoDocumento == aprendiz.Persona.IdTipoDocumento)
            .Select(t => t.Nombre)
            .FirstOrDefaultAsync() ?? "";

        var fichasDto = new List<FichaPerfilDTO>();
        foreach (var fa in aprendiz.FichaAprendices)
        {
            var fichaCompetencias = await db.FichaCompetencia
                .Include(fc => fc.Competencia)
                .Where(fc => fc.IdFicha == fa.IdFicha)
                .Select(fc => new { fc.IdFichaCompetencia, fc.Competencia.Nombre, fc.Estado, fc.TotalHoras })
                .ToListAsync();

            var idsFichaCompetencia = fichaCompetencias.Select(fc => fc.IdFichaCompetencia).ToList();

            var resultadosProgramados = await db.FichaCompetenciaResultado
                .Include(fcr => fcr.Resultado)
                .Include(fcr => fcr.Instructor!).ThenInclude(i => i.Usuario).ThenInclude(u => u.Persona)
                .Where(fcr => idsFichaCompetencia.Contains(fcr.IdFichaCompetencia))
                .ToListAsync();

            var idsResultado = resultadosProgramados.Select(fcr => fcr.IdResultado).ToList();

            var juicios = await db.JuicioResultado
                .Where(j => j.IdAprendiz == aprendiz.IdAprendiz && idsResultado.Contains(j.IdResultado))
                .ToListAsync();

            var competencias = fichaCompetencias.Select(fc => new CompetenciaPerfilDTO
            {
                Nombre = fc.Nombre,
                Estado = fc.Estado,
                TotalHoras = fc.TotalHoras,
                Resultados = resultadosProgramados
                    .Where(fcr => fcr.IdFichaCompetencia == fc.IdFichaCompetencia)
                    .Select(fcr => new ResultadoPerfilDTO
                    {
                        Codigo = fcr.Resultado.Codigo,
                        Nombre = fcr.Resultado.Nombre,
                        EstadoProgramacion = fcr.Estado,
                        NombreInstructor = fcr.Instructor is null ? null
                            : $"{fcr.Instructor.Usuario.Persona.Nombres} {fcr.Instructor.Usuario.Persona.Apellidos}",
                        Juicio = juicios.FirstOrDefault(j => j.IdResultado == fcr.IdResultado)?.Juicio ?? "Sin evaluar"
                    })
                    .ToList()
            }).ToList();

            fichasDto.Add(new FichaPerfilDTO
            {
                NumeroFicha = fa.Ficha.NumeroFicha,
                EstadoFicha = fa.Ficha.Estado,
                EstadoAprendizEnFicha = fa.Estado,
                Competencias = competencias
            });
        }

        var estadoAprendiz = aprendiz.FichaAprendices
            .FirstOrDefault(fa => fa.Estado == "EN FORMACION")?.Estado
            ?? aprendiz.FichaAprendices.FirstOrDefault()?.Estado
            ?? "";

        return new PerfilAprendizDTO
        {
            IdAprendiz                 = aprendiz.IdAprendiz,
            TipoDocumento              = tipoDoc,
            NumeroDocumento            = aprendiz.Persona.NumeroDocumento,
            Nombres                    = aprendiz.Persona.Nombres,
            Apellidos                  = aprendiz.Persona.Apellidos,
            Correo                     = aprendiz.Usuario?.Correo ?? "",
            Telefono                   = aprendiz.Persona.Telefono,
            Direccion                  = aprendiz.Persona.Direccion,
            Municipio                  = aprendiz.Persona.Municipio,
            FechaNacimiento            = aprendiz.Persona.FechaNacimiento,
            Estrato                    = aprendiz.Estrato,
            Estado                     = estadoAprendiz,
            CondicionEspecial          = aprendiz.CondicionEspecial,
            TipoPoblacion              = aprendiz.TipoPoblacion,
            ContactoEmergenciaNombre   = aprendiz.ContactoEmergenciaNombre,
            ContactoEmergenciaTelefono = aprendiz.ContactoEmergenciaTelefono,
            Fichas                     = fichasDto
        };
    }

    public async Task<bool> ActualizarContactoAsync(int idUsuario, ActualizarContactoDTO dto)
    {
        var aprendiz = await db.AprendizPerfil
            .Include(a => a.Persona)
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

        if (aprendiz is null) return false;

        aprendiz.Persona.Telefono       = dto.Telefono;
        aprendiz.Persona.Direccion      = dto.Direccion;
        aprendiz.Persona.Municipio      = dto.Municipio;
        aprendiz.Persona.FechaNacimiento = dto.FechaNacimiento;

        aprendiz.Estrato                    = dto.Estrato;
        aprendiz.TipoPoblacion              = dto.TipoPoblacion;
        aprendiz.CondicionEspecial          = dto.CondicionEspecial;
        aprendiz.ContactoEmergenciaNombre   = dto.ContactoEmergenciaNombre;
        aprendiz.ContactoEmergenciaTelefono = dto.ContactoEmergenciaTelefono;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<CronogramaAprendizDTO?> ObtenerCronogramaAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

        if (aprendiz is null) return null;

        var idFichas = await db.FichaAprendiz
            .Where(fa => fa.IdAprendiz == aprendiz.IdAprendiz)
            .Select(fa => fa.IdFicha)
            .ToListAsync();

        // Clases programadas: FCR con instructor y horario asignado (no Pendientes)
        var resultados = await db.FichaCompetenciaResultado
            .Include(fcr => fcr.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .Include(fcr => fcr.FichaCompetencia).ThenInclude(fc => fc.Ficha)
            .Include(fcr => fcr.Instructor!).ThenInclude(i => i.Usuario).ThenInclude(u => u.Persona)
            .Where(fcr => idFichas.Contains(fcr.FichaCompetencia.IdFicha)
                       && fcr.Estado != "Pendiente"
                       && fcr.HoraInicio != null
                       && fcr.FechaInicio != null)
            .OrderBy(fcr => fcr.FechaInicio).ThenBy(fcr => fcr.HoraInicio)
            .ToListAsync();

        // Expandir cada FCR en un bloque por día (FechaInicio → FechaFin, excl. domingos)
        var sesionesDto = new List<SesionCronogramaDTO>();
        foreach (var fcr in resultados)
        {
            var inicio = fcr.FechaInicio!.Value;
            var fin    = fcr.FechaFin ?? inicio;
            var dias   = Math.Min((int)(fin.ToDateTime(TimeOnly.MinValue)
                                      - inicio.ToDateTime(TimeOnly.MinValue)).TotalDays, 180);
            var nombreInstructor = fcr.Instructor is null ? "—"
                : $"{fcr.Instructor.Usuario.Persona.Nombres} {fcr.Instructor.Usuario.Persona.Apellidos}";

            for (int i = 0; i <= dias; i++)
            {
                var fecha = inicio.AddDays(i);
                if (fecha.DayOfWeek == DayOfWeek.Sunday) continue;

                sesionesDto.Add(new SesionCronogramaDTO
                {
                    NumeroFicha       = fcr.FichaCompetencia.Ficha.NumeroFicha,
                    NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                    Fecha             = fecha,
                    HoraInicio        = fcr.HoraInicio!.Value,
                    HoraFin           = fcr.HoraFin ?? fcr.HoraInicio!.Value,
                    NombreInstructor  = nombreInstructor,
                    EstadoSesion      = fcr.Estado
                });
            }
        }

        var patrocinio = await db.Patrocinio
            .Where(p => p.IdAprendiz == aprendiz.IdAprendiz && p.Activo && p.HoraInicio != null)
            .Select(p => new PatrocinioCronogramaDTO
            {
                NombreEmpresa    = p.NombreEmpresa ?? "",
                Etapa            = p.Etapa,
                HoraInicio       = p.HoraInicio,
                HoraFin          = p.HoraFin,
                FechaInicioEtapa = p.FechaInicioEtapa,
                FechaFinEtapa    = p.FechaFinEtapa
            })
            .FirstOrDefaultAsync();

        return new CronogramaAprendizDTO
        {
            Sesiones   = sesionesDto,
            Patrocinio = patrocinio
        };
    }
}
