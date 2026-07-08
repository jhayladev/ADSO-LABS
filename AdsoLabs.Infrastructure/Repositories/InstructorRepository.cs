using AdsoLabs.Application.Commands.Instructor;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Core.Enums;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class InstructorRepository(AdsoDbContext context) : IInstructorRepository
{
    public async Task<ResultadoAgregar> AgregarAsync(AgregarInstructorCommand command)
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

            var correoExistente = await context.Usuario
                .AnyAsync(u => u.Correo == command.Correo);
            if (correoExistente)
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
                FechaNacimiento = command.FechaNacimiento.HasValue
                    ? DateOnly.FromDateTime(command.FechaNacimiento.Value)
                    : null,
                Direccion = command.Direccion,
                Municipio = command.Municipio,
                FechaCreacion = DateTime.Now
            };
            await context.Persona.AddAsync(persona);
            await context.SaveChangesAsync();

            var rolInstructor = await context.Rol.FirstAsync(r => r.Nombre == "Instructor");

            var usuario = new Usuario
            {
                IdPersona = persona.IdPersona,
                IdRol = rolInstructor.IdRol,
                Correo = command.Correo,
                PasswordHash = null,
                PasswordSalt = null,
                Activo = true,
                PrimerLogin = true,
                FechaCreacion = DateTime.Now
            };
            await context.Usuario.AddAsync(usuario);
            await context.SaveChangesAsync();

            var perfil = new InstructorPerfil
            {
                IdUsuario = usuario.IdUsuario,
                NumeroContrato = command.NumeroContrato,
                Especialidad = command.Especialidad,
                TipoVinculacion = command.TipoVinculacion,
                FechaVinculacion = command.FechaVinculacion,
                Activo = true
            };
            await context.InstructorPerfil.AddAsync(perfil);
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

    public async Task<bool> EditarAsync(int idInstructor)
    {
        var perfil = await context.InstructorPerfil.FindAsync(idInstructor);
        if (perfil == null) return false;
        perfil.Activo = !perfil.Activo;
        await context.SaveChangesAsync();
        return true;
    }
}
