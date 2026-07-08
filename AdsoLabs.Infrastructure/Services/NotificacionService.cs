using AdsoLabs.Application.DTOs.Notificacion;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Services;

public class NotificacionService(
    AdsoDbContext context,
    INotificacionRepository repo,
    INotificacionPusher pusher) : INotificacionService
{
    // ── Escenario 1: Observación agregada ───────────────────────────────────────
    public async Task NotificarObservacionAgregadaAsync(int idAprendiz, int idUsuarioCreador)
    {
        var aprendiz = await context.AprendizPerfil
            .Include(a => a.Persona)
            .FirstOrDefaultAsync(a => a.IdAprendiz == idAprendiz);
        if (aprendiz is null) return;

        string nombreAprendiz = $"{aprendiz.Persona.Nombres} {aprendiz.Persona.Apellidos}";

        var creador = await context.Usuario
            .Include(u => u.Persona)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuarioCreador);
        string nombreCreador = creador is not null
            ? $"{creador.Persona.Nombres} {creador.Persona.Apellidos}"
            : "Un usuario";
        var destinatarios = new List<(int IdUsuario, string Titulo, string Mensaje)>();

        if (aprendiz.IdUsuario.HasValue && aprendiz.IdUsuario.Value != idUsuarioCreador)
            destinatarios.Add((aprendiz.IdUsuario.Value,
                "Nueva observación",
                $"{nombreCreador} agregó una observación en tu informe."));

        var instructoresIds = await context.FichaAprendiz
            .Where(fa => fa.IdAprendiz == idAprendiz && fa.Estado == "EN FORMACION")
            .SelectMany(fa => context.FichaCompetenciaResultado
                .Where(fcr => fcr.FichaCompetencia.IdFicha == fa.IdFicha && fcr.IdInstructor != null)
                .Select(fcr => fcr.Instructor!.IdUsuario))
            .Distinct()
            .Where(id => id != idUsuarioCreador)
            .ToListAsync();

        foreach (var id in instructoresIds)
            destinatarios.Add((id,
                "Observación registrada",
                $"{nombreCreador} agregó una observación sobre {nombreAprendiz}."));

        await EnviarATodosAsync("ObservacionAgregada", destinatarios, idAprendiz, "Aprendiz");
    }

    // ── Escenario 2 (por idAprendiz): Datos personales actualizados ─────────────
    public async Task NotificarDatosPersonalesActualizadosAsync(int idAprendiz)
    {
        var aprendiz = await context.AprendizPerfil
            .Include(a => a.Persona)
            .FirstOrDefaultAsync(a => a.IdAprendiz == idAprendiz);
        if (aprendiz is null) return;

        string nombre = $"{aprendiz.Persona.Nombres} {aprendiz.Persona.Apellidos}";
        await NotificarDatosPersonalesInternoAsync(idAprendiz, nombre);
    }

    // ── Escenario 2 (por idUsuario): Datos personales actualizados ──────────────
    public async Task NotificarDatosPersonalesActualizadosPorUsuarioAsync(int idUsuario)
    {
        var aprendiz = await context.AprendizPerfil
            .Include(a => a.Persona)
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);
        if (aprendiz is null) return;

        string nombre = $"{aprendiz.Persona.Nombres} {aprendiz.Persona.Apellidos}";
        await NotificarDatosPersonalesInternoAsync(aprendiz.IdAprendiz, nombre);
    }

    private async Task NotificarDatosPersonalesInternoAsync(int idAprendiz, string nombreAprendiz)
    {
        const string titulo  = "Actualización de datos";
        string       mensaje = $"{nombreAprendiz} actualizó su información personal.";

        var destinatarios = new List<(int IdUsuario, string Titulo, string Mensaje)>();

        var instructoresIds = await context.FichaAprendiz
            .Where(fa => fa.IdAprendiz == idAprendiz && fa.Estado == "EN FORMACION")
            .SelectMany(fa => context.FichaCompetenciaResultado
                .Where(fcr => fcr.FichaCompetencia.IdFicha == fa.IdFicha && fcr.IdInstructor != null)
                .Select(fcr => fcr.Instructor!.IdUsuario))
            .Distinct()
            .ToListAsync();

        foreach (var id in instructoresIds)
            destinatarios.Add((id, titulo, mensaje));

        var adminsIds = await context.Usuario
            .Where(u => u.Rol.Nombre == "Administrador" && u.Activo)
            .Select(u => u.IdUsuario)
            .ToListAsync();

        foreach (var id in adminsIds)
            if (!destinatarios.Any(d => d.IdUsuario == id))
                destinatarios.Add((id, titulo, mensaje));

        await EnviarATodosAsync("DatosPersonalesActualizados", destinatarios, idAprendiz, "Aprendiz");
    }

    // ── Escenario 3: Resultado programado ───────────────────────────────────────
    public async Task NotificarResultadoProgramadoAsync(
        string numeroFicha, int idCompetencia, int idResultado, int idInstructor)
    {
        var info = await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.FichaCompetencia.Ficha.NumeroFicha == numeroFicha &&
                fcr.FichaCompetencia.IdCompetencia     == idCompetencia &&
                fcr.IdResultado                        == idResultado)
            .Select(fcr => new
            {
                IdFicha             = fcr.FichaCompetencia.IdFicha,
                fcr.IdFichaCompetenciaResultado,
                NombreCompetencia   = fcr.FichaCompetencia.Competencia.Nombre,
                NombreResultado     = fcr.Resultado.Nombre,
                IdUsuarioInstructor = fcr.Instructor!.IdUsuario
            })
            .FirstOrDefaultAsync();

        if (info is null) return;

        var destinatarios = new List<(int IdUsuario, string Titulo, string Mensaje)>
        {
            (info.IdUsuarioInstructor,
             "Resultado asignado",
             $"Se te asignó el resultado '{info.NombreResultado}' de '{info.NombreCompetencia}' en la ficha {numeroFicha}.")
        };

        var aprendicesIds = await context.FichaAprendiz
            .Where(fa => fa.IdFicha == info.IdFicha && fa.Estado == "EN FORMACION")
            .Select(fa => fa.Aprendiz.IdUsuario)
            .Where(id => id != null)
            .Select(id => id!.Value)
            .ToListAsync();

        foreach (var id in aprendicesIds)
            destinatarios.Add((id,
                "Nuevo resultado programado",
                $"El resultado '{info.NombreResultado}' fue programado en tu ficha {numeroFicha}."));

        await EnviarATodosAsync("ResultadoAsignado", destinatarios,
            info.IdFichaCompetenciaResultado, "FichaCompetenciaResultado");
    }

    // ── Helper ──────────────────────────────────────────────────────────────────
    private async Task EnviarATodosAsync(
        string tipo,
        List<(int IdUsuario, string Titulo, string Mensaje)> destinatarios,
        int? referenciaId,
        string? referenciaTipo)
    {
        foreach (var (idUsuario, titulo, mensaje) in destinatarios)
        {
            var idNotificacion = await repo.CrearAsync(
                idUsuario, tipo, titulo, mensaje, referenciaId, referenciaTipo);

            await pusher.EnviarAsync(idUsuario, new NotificacionDTO
            {
                IdNotificacion = idNotificacion,
                Tipo           = tipo,
                Titulo         = titulo,
                Mensaje        = mensaje,
                Leida          = false,
                FechaCreacion  = DateTime.Now,
                ReferenciaId   = referenciaId,
                ReferenciaTipo = referenciaTipo
            });
        }
    }
}
