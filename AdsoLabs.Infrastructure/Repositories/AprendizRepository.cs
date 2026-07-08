using AdsoLabs.Application.Commands.Aprendiz;
using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Core.Enums;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class AprendizRepository(AdsoDbContext context) : IAprendizRepository
{
    public async Task<ResultadoAgregar> AgregarAsync(AgregarAprendizCommand command)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var personaExistente = await context.Persona
                .FirstOrDefaultAsync(p => p.NumeroDocumento == command.NumeroDocumento);
            if (personaExistente != null)
            {
                await transaction.RollbackAsync();
                return ResultadoAgregar.Existe;
            }

            var tipoDoc = await context.TipoDocumento
                .FirstOrDefaultAsync(t => t.Nombre == command.TipoDocumento);
            if (tipoDoc == null)
            {
                await transaction.RollbackAsync();
                return ResultadoAgregar.Fallido;
            }

            var persona = new Persona
            {
                IdTipoDocumento = tipoDoc.IdTipoDocumento,
                NumeroDocumento = command.NumeroDocumento,
                Nombres = command.Nombre,
                Apellidos = command.Apellido,
                Telefono = command.Telefono,
                FechaNacimiento = DateOnly.FromDateTime(command.FechaNacimiento),
                Direccion = command.Direccion,
                Municipio = command.Municipio,
                FechaCreacion = DateTime.Now
            };
            await context.Persona.AddAsync(persona);
            await context.SaveChangesAsync();

            var ficha = await context.Ficha
                .FirstOrDefaultAsync(f => f.NumeroFicha == command.NumeroFicha);
            if (ficha == null)
            {
                await transaction.RollbackAsync();
                return ResultadoAgregar.Fallido;
            }

            var perfilAprendiz = new AprendizPerfil
            {
                IdPersona = persona.IdPersona,
                Estrato = command.Estrato,
                CondicionEspecial = command.CondicionEspecial,
                TipoPoblacion = command.TipoPoblacion,
                ContactoEmergenciaNombre = command.ContactoEmergenciaNombre,
                ContactoEmergenciaTelefono = command.ContactoEmergenciaTelefono,
                FechaRegistro = DateTime.Now
            };
            await context.AprendizPerfil.AddAsync(perfilAprendiz);
            await context.SaveChangesAsync();

            context.FichaAprendiz.Add(new FichaAprendiz
            {
                IdFicha = ficha.IdFicha,
                IdAprendiz = perfilAprendiz.IdAprendiz,
                Estado = EstadosAprendiz.EnFormacion
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoAgregar.Exitoso;
        }
        catch
        {
            await transaction.RollbackAsync();
            return ResultadoAgregar.Fallido;
        }
    }

    public async Task<string?> ObtenerEstadoActualAsync(int idAprendiz, int idFicha)
        => await context.FichaAprendiz
            .Where(fa => fa.IdAprendiz == idAprendiz && fa.IdFicha == idFicha)
            .Select(fa => fa.Estado)
            .FirstOrDefaultAsync();

    public async Task<bool> EditarEstadoAsync(int idAprendiz, int idFicha, string estado, int? idUsuarioCambio)
    {
        var fichaAprendiz = await context.FichaAprendiz
            .FirstOrDefaultAsync(fa => fa.IdAprendiz == idAprendiz && fa.IdFicha == idFicha);
        if (fichaAprendiz == null) return false;

        var estadoAnterior = fichaAprendiz.Estado;
        fichaAprendiz.Estado = estado;

        context.HistorialEstadoAprendiz.Add(new HistorialEstadoAprendiz
        {
            IdFicha = idFicha,
            IdAprendiz = idAprendiz,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estado,
            IdUsuarioCambio = idUsuarioCambio,
            FechaCambio = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return true;
    }

    public async Task AsignarUsuarioAsync(int idAprendiz, int idUsuario)
    {
        var perfil = await context.AprendizPerfil.FindAsync(idAprendiz);
        if (perfil != null)
        {
            perfil.IdUsuario = idUsuario;
            await context.SaveChangesAsync();
        }
    }

    public Task<string?> ObtenerEstadoMasRecienteAsync(int idAprendiz)
        => context.FichaAprendiz
            .Where(fa => fa.IdAprendiz == idAprendiz)
            .OrderByDescending(fa => fa.Ficha.FechaInicio)
            .Select(fa => fa.Estado)
            .FirstOrDefaultAsync();

    public async Task<Core.Entities.AprendizPerfil?> ObtenerPorIdPersonaAsync(int idPersona)
    {
        var p = await context.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdPersona == idPersona);
        if (p == null) return null;
        return new Core.Entities.AprendizPerfil
        {
            IdAprendiz = p.IdAprendiz,
            IdPersona = p.IdPersona,
            IdUsuario = p.IdUsuario,
            FechaRegistro = p.FechaRegistro
        };
    }
}
