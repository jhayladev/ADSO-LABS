using AdsoLabs.Application.DTOs.Notificacion;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class NotificacionRepository(AdsoDbContext context) : INotificacionRepository
{
    public Task<List<NotificacionDTO>> ObtenerPorUsuarioAsync(int idUsuario, int cantidad = 50)
        => context.Notificacion
            .Where(n => n.IdUsuarioDestinatario == idUsuario)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(cantidad)
            .Select(n => new NotificacionDTO
            {
                IdNotificacion = n.IdNotificacion,
                Tipo           = n.Tipo,
                Titulo         = n.Titulo,
                Mensaje        = n.Mensaje,
                Leida          = n.Leida,
                FechaCreacion  = n.FechaCreacion,
                ReferenciaId   = n.ReferenciaId,
                ReferenciaTipo = n.ReferenciaTipo
            })
            .ToListAsync();

    public Task<int> ContarNoLeidasAsync(int idUsuario)
        => context.Notificacion
            .CountAsync(n => n.IdUsuarioDestinatario == idUsuario && !n.Leida);

    public async Task MarcarLeidaAsync(int idNotificacion, int idUsuario)
    {
        var n = await context.Notificacion
            .FirstOrDefaultAsync(x => x.IdNotificacion == idNotificacion
                                   && x.IdUsuarioDestinatario == idUsuario);
        if (n is null || n.Leida) return;
        n.Leida        = true;
        n.FechaLectura = DateTime.Now;
        await context.SaveChangesAsync();
    }

    public Task MarcarTodasLeidasAsync(int idUsuario)
        => context.Notificacion
            .Where(n => n.IdUsuarioDestinatario == idUsuario && !n.Leida)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Leida, true)
                .SetProperty(n => n.FechaLectura, DateTime.Now));

    public async Task<int> CrearAsync(int idUsuarioDestinatario, string tipo, string titulo,
        string mensaje, int? referenciaId = null, string? referenciaTipo = null)
    {
        var n = new Notificacion
        {
            IdUsuarioDestinatario = idUsuarioDestinatario,
            Tipo           = tipo,
            Titulo         = titulo,
            Mensaje        = mensaje,
            Leida          = false,
            FechaCreacion  = DateTime.Now,
            ReferenciaId   = referenciaId,
            ReferenciaTipo = referenciaTipo
        };
        context.Notificacion.Add(n);
        await context.SaveChangesAsync();
        return n.IdNotificacion;
    }
}
