using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class UsuarioRepository(AdsoDbContext context) : IUsuarioRepository
{
    public async Task<Core.Entities.Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var u = await context.Usuario
            .Include(u => u.Rol)
            .Include(u => u.Persona)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower());

        if (u == null) return null;

        // Carga explícita como fallback: en relaciones one-to-one la proyección del Include
        // puede llegar null por tracking de EF; si ocurre, fuerza la carga separada.
        if (u.Persona == null && u.IdPersona > 0)
            await context.Entry(u).Reference(x => x.Persona).LoadAsync();

        return new Core.Entities.Usuario
        {
            IdUsuario = u.IdUsuario,
            IdPersona = u.IdPersona,
            IdRol = u.IdRol,
            Correo = u.Correo,
            PasswordHash = u.PasswordHash,
            PasswordSalt = u.PasswordSalt,
            Activo = u.Activo,
            PrimerLogin = u.PrimerLogin,
            IntentosFallidos = u.IntentosFallidos,
            BloqueadoHasta = u.BloqueadoHasta,
            FechaCreacion = u.FechaCreacion,
            FechaInicioSesion = u.FechaInicioSesion,
            Rol = u.Rol == null ? null : new Core.Entities.Rol
            {
                IdRol = u.Rol.IdRol,
                Nombre = u.Rol.Nombre
            },
            Persona = u.Persona == null ? null : new Core.Entities.Persona
            {
                IdPersona = u.Persona.IdPersona,
                NumeroDocumento = u.Persona.NumeroDocumento,
                Nombres = u.Persona.Nombres,
                Apellidos = u.Persona.Apellidos
            }
        };
    }

    public async Task<Core.Entities.Usuario?> ObtenerPorIdAsync(int id)
    {
        var u = await context.Usuario.FindAsync(id);
        if (u == null) return null;
        return new Core.Entities.Usuario
        {
            IdUsuario = u.IdUsuario,
            IdPersona = u.IdPersona,
            IdRol = u.IdRol,
            Correo = u.Correo,
            PasswordHash = u.PasswordHash,
            PasswordSalt = u.PasswordSalt,
            Activo = u.Activo,
            PrimerLogin = u.PrimerLogin,
            IntentosFallidos = u.IntentosFallidos,
            BloqueadoHasta = u.BloqueadoHasta,
            FechaCreacion = u.FechaCreacion,
            FechaInicioSesion = u.FechaInicioSesion
        };
    }

    public async Task<Core.Entities.Usuario?> ObtenerPorIdPersonaAsync(int idPersona)
    {
        var u = await context.Usuario.FirstOrDefaultAsync(u => u.IdPersona == idPersona);
        if (u == null) return null;
        return new Core.Entities.Usuario
        {
            IdUsuario = u.IdUsuario,
            IdPersona = u.IdPersona,
            IdRol = u.IdRol,
            Correo = u.Correo,
            Activo = u.Activo,
            PrimerLogin = u.PrimerLogin,
            IntentosFallidos = u.IntentosFallidos,
            FechaCreacion = u.FechaCreacion
        };
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
        => await context.Usuario.AnyAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Core.Entities.Usuario domainUsuario)
    {
        var efUsuario = new Models.Usuario
        {
            IdPersona = domainUsuario.IdPersona,
            IdRol = domainUsuario.IdRol,
            Correo = domainUsuario.Correo,
            PasswordHash = domainUsuario.PasswordHash,
            PasswordSalt = domainUsuario.PasswordSalt,
            Activo = domainUsuario.Activo,
            PrimerLogin = domainUsuario.PrimerLogin,
            IntentosFallidos = domainUsuario.IntentosFallidos,
            FechaCreacion = domainUsuario.FechaCreacion
        };
        context.Usuario.Add(efUsuario);
        await context.SaveChangesAsync();
        domainUsuario.IdUsuario = efUsuario.IdUsuario;
    }

    public async Task ActualizarLoginAsync(int idUsuario, int intentosFallidos, DateTime? bloqueadoHasta, DateTime? fechaInicioSesion)
    {
        var u = await context.Usuario.FindAsync(idUsuario);
        if (u == null) return;
        u.IntentosFallidos = intentosFallidos;
        u.BloqueadoHasta = bloqueadoHasta;
        if (fechaInicioSesion.HasValue)
            u.FechaInicioSesion = fechaInicioSesion;
        await context.SaveChangesAsync();
    }

    public async Task CambiarPasswordAsync(int idUsuario, byte[] hash, byte[] salt, bool primerLogin, DateTime? consentimientoFecha = null)
    {
        var u = await context.Usuario.FindAsync(idUsuario);
        if (u == null) return;
        u.PasswordHash = hash;
        u.PasswordSalt = salt;
        u.PrimerLogin = primerLogin;
        if (consentimientoFecha.HasValue)
            u.ConsentimientoFecha = consentimientoFecha;
        await context.SaveChangesAsync();
    }

    public async Task ActivarAsync(int idUsuario)
    {
        var u = await context.Usuario.FindAsync(idUsuario);
        if (u == null) return;
        u.Activo = true;
        u.PrimerLogin = true;
        await context.SaveChangesAsync();
    }

    public async Task ActualizarCorreoAsync(int idUsuario, string correo)
    {
        var u = await context.Usuario.FindAsync(idUsuario);
        if (u == null) return;
        u.Correo = correo;
        await context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarContactoAsync(int idUsuario, string correo, string? telefono)
    {
        var correoDuplicado = await context.Usuario
            .AnyAsync(u => u.Correo.ToLower() == correo.ToLower() && u.IdUsuario != idUsuario);
        if (correoDuplicado) return false;

        var u = await context.Usuario.FindAsync(idUsuario);
        if (u == null) return false;

        u.Correo = correo;

        var persona = await context.Persona.FindAsync(u.IdPersona);
        if (persona != null)
            persona.Telefono = telefono;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int?> ObtenerIdInstructorPorUsuarioAsync(int idUsuario)
    {
        var perfil = await context.InstructorPerfil
            .FirstOrDefaultAsync(i => i.IdUsuario == idUsuario);
        return perfil?.IdInstructor;
    }
}
