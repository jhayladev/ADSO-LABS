using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IAprendizService"/>.
///
/// Centraliza la lógica de persistencia de Persona, AprendizPerfil y FichaAprendiz.
/// Es utilizado por el importador de aprendices (Prioridad 4 del plan) y por cualquier
/// otro módulo que necesite crear o actualizar aprendices sin duplicar código.
///
/// NOTA ARQUITECTÓNICA:
///   El importador de juicios SOFIA NO usa este servicio para crear aprendices.
///   SOFIA solo consulta si el aprendiz existe; si no existe, registra el error
///   y continúa con la siguiente fila.
/// </summary>
public class AprendizService(AdsoDbContext db) : IAprendizService
{
    // ═══════════════════════════════════════════════════════════════════════
    //  PERSONA
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<int> UpsertPersonaAsync(
        string numeroDocumento,
        string nombres,
        string apellidos,
        int    idTipoDocumento)
    {
        // Buscar por número de documento (clave natural única de Persona)
        var existing = await db.Persona
            .FirstOrDefaultAsync(p => p.NumeroDocumento == numeroDocumento);

        if (existing is not null)
        {
            // El archivo de importación es fuente de verdad para nombres y apellidos.
            // Se actualizan siempre para corregir errores tipográficos o cambios legales.
            existing.Nombres   = nombres;
            existing.Apellidos = apellidos;
            await db.SaveChangesAsync();
            return existing.IdPersona;
        }

        // Primera aparición: crear el registro completo
        var persona = new Persona
        {
            IdTipoDocumento = idTipoDocumento,
            NumeroDocumento = numeroDocumento,
            Nombres         = nombres,
            Apellidos       = apellidos
        };
        db.Persona.Add(persona);
        await db.SaveChangesAsync();
        return persona.IdPersona;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  APRENDIZ PERFIL
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<(int idAprendiz, bool isNew)> UpsertAprendizPerfilAsync(int idPersona)
    {
        // Si ya existe un perfil para esta persona, devolverlo sin modificar.
        // El perfil del aprendiz no tiene datos adicionales que actualizar en este punto;
        // campos como teléfono o dirección se gestionan desde el módulo de perfil.
        var existing = await db.AprendizPerfil
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdPersona == idPersona);

        if (existing is not null)
            return (existing.IdAprendiz, false);

        var perfil = new AprendizPerfil { IdPersona = idPersona };
        db.AprendizPerfil.Add(perfil);
        await db.SaveChangesAsync();
        return (perfil.IdAprendiz, true);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  VINCULACIÓN APRENDIZ-FICHA
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task VincularAprendizFichaAsync(
        int    idFicha,
        int    idAprendiz,
        string estadoAprendiz)
    {
        var existing = await db.FichaAprendiz
            .FirstOrDefaultAsync(fa => fa.IdFicha == idFicha && fa.IdAprendiz == idAprendiz);

        if (existing is not null)
        {
            // Sin cambio: no emitir un UPDATE innecesario a la BD
            if (existing.Estado == estadoAprendiz) return;

            // Caso especial: el Excel dice EN FORMACION pero la BD tiene TRASLADADO.
            // Si existe una ficha más reciente donde el aprendiz está activo, el traslado
            // ya ocurrió y no debe revertirse aunque el Excel histórico lo indique.
            // (CLAUDE.md §7.9 — regla de traslado automático)
            if (existing.Estado == EstadosAprendiz.Trasladado &&
                estadoAprendiz  == EstadosAprendiz.EnFormacion)
            {
                var fechaActual = await db.Ficha
                    .Where(f => f.IdFicha == idFicha)
                    .Select(f => f.FechaInicio)
                    .FirstAsync();

                bool hayFichaMasReciente = await db.FichaAprendiz
                    .Where(fa => fa.IdAprendiz == idAprendiz && fa.IdFicha != idFicha)
                    .Join(db.Ficha, fa => fa.IdFicha, f => f.IdFicha, (fa, f) => f.FechaInicio)
                    .AnyAsync(fecha => fecha > fechaActual);

                // Conservar el traslado si existe una ficha más reciente activa
                if (hayFichaMasReciente) return;
            }

            existing.Estado = estadoAprendiz;
            await db.SaveChangesAsync();
            return;
        }

        // Primera vinculación: si el aprendiz llega EN FORMACION, marcar como
        // TRASLADADO las fichas anteriores donde estuviera activo (CLAUDE.md §7.9).
        if (estadoAprendiz == EstadosAprendiz.EnFormacion)
        {
            var anteriores = await db.FichaAprendiz
                .Where(fa => fa.IdAprendiz == idAprendiz
                          && fa.IdFicha    != idFicha
                          && fa.Estado     == EstadosAprendiz.EnFormacion)
                .ToListAsync();

            foreach (var fa in anteriores)
                fa.Estado = EstadosAprendiz.Trasladado;
        }

        db.FichaAprendiz.Add(new FichaAprendiz
        {
            IdFicha    = idFicha,
            IdAprendiz = idAprendiz,
            Estado     = estadoAprendiz
        });
        await db.SaveChangesAsync();
    }
}
