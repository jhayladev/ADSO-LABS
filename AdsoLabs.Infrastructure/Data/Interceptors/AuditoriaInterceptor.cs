using System.Security.Claims;
using System.Text.Json;
using AdsoLabs.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdsoLabs.Infrastructure.Data.Interceptors;

/// <summary>
/// Registra en adso.Auditoria los cambios de las entidades sensibles (perfil de aprendiz/instructor,
/// estado de FichaAprendiz, carga/eliminación de archivos, cambio de contraseña de Usuario).
/// Patrón de dos fases: en SavingChangesAsync arma las filas de auditoría en memoria (antes de guardar,
/// para poder leer OriginalValues); en SavedChangesAsync completa el Id ya generado y hace un segundo
/// SaveChanges solo para esas filas (Auditoria no está en el set de entidades auditadas, así que no
/// hay recursión infinita: la segunda pasada no encuentra nada que auditar).
/// </summary>
public class AuditoriaInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private static readonly HashSet<string> PropiedadesSensiblesUsuario = new() { "PasswordHash", "PasswordSalt" };

    private List<(EntityEntry Entry, Auditoria Auditoria)> _pendientes = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _pendientes = [];

        var context = eventData.Context;
        if (context != null)
        {
            var idUsuario = ObtenerIdUsuarioActual();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                var auditoria = ConstruirAuditoria(entry, idUsuario);
                if (auditoria != null)
                    _pendientes.Add((entry, auditoria));
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_pendientes.Count > 0 && eventData.Context != null)
        {
            var pendientes = _pendientes;
            _pendientes = [];

            foreach (var (entry, auditoria) in pendientes)
                auditoria.IdRegistroAfectado = ObtenerIdRegistroAfectado(entry);

            eventData.Context.Set<Auditoria>().AddRange(pendientes.Select(p => p.Auditoria));
            await eventData.Context.SaveChangesAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static Auditoria? ConstruirAuditoria(EntityEntry entry, int? idUsuario)
    {
        if (entry.State is EntityState.Unchanged or EntityState.Detached)
            return null;

        string tabla;
        HashSet<string>? propiedadesExcluidas = null;

        switch (entry.Entity)
        {
            case AprendizPerfil:
                tabla = "AprendizPerfil";
                break;
            case InstructorPerfil:
                tabla = "InstructorPerfil";
                break;
            case FichaAprendiz when entry.State == EntityState.Modified:
                tabla = "FichaAprendiz";
                break;
            case Archivo when entry.State is EntityState.Added or EntityState.Deleted:
                tabla = "Archivo";
                break;
            case Usuario when entry.State == EntityState.Modified
                && entry.Property(nameof(Usuario.PasswordHash)).IsModified:
                tabla = "Usuario";
                propiedadesExcluidas = PropiedadesSensiblesUsuario;
                break;
            default:
                return null;
        }

        return new Auditoria
        {
            IdUsuario = idUsuario,
            Accion = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Deleted => "DELETE",
                _ => "UPDATE"
            },
            TablaAfectada = tabla,
            IdRegistroAfectado = string.Empty, // se completa en SavedChangesAsync, cuando ya hay Id generado
            DatosViejos = entry.State != EntityState.Added ? SerializarValores(entry.OriginalValues, propiedadesExcluidas) : null,
            DatosNuevos = entry.State != EntityState.Deleted ? SerializarValores(entry.CurrentValues, propiedadesExcluidas) : null,
            Fecha = DateTime.UtcNow
        };
    }

    private static string SerializarValores(PropertyValues valores, HashSet<string>? propiedadesExcluidas)
    {
        var dict = valores.Properties
            .Where(p => propiedadesExcluidas == null || !propiedadesExcluidas.Contains(p.Name))
            .ToDictionary(p => p.Name, p => valores[p]);
        return JsonSerializer.Serialize(dict);
    }

    private static string ObtenerIdRegistroAfectado(EntityEntry entry)
    {
        var clave = entry.Metadata.FindPrimaryKey();
        if (clave == null) return string.Empty;

        var valores = clave.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? string.Empty);
        return string.Join("-", valores);
    }

    private int? ObtenerIdUsuarioActual()
    {
        var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var idUsuario) ? idUsuario : null;
    }
}
