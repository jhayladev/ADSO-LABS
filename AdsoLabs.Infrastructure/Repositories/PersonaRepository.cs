using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class PersonaRepository(AdsoDbContext context) : IPersonaRepository
{
    public async Task<Core.Entities.Persona?> ObtenerPorNumeroDocumentoAsync(string numeroDocumento)
    {
        var p = await context.Persona.FirstOrDefaultAsync(p => p.NumeroDocumento == numeroDocumento);
        if (p == null) return null;
        return new Core.Entities.Persona
        {
            IdPersona = p.IdPersona,
            IdTipoDocumento = p.IdTipoDocumento,
            NumeroDocumento = p.NumeroDocumento,
            Nombres = p.Nombres,
            Apellidos = p.Apellidos,
            Telefono = p.Telefono,
            FechaNacimiento = p.FechaNacimiento,
            Direccion = p.Direccion,
            Municipio = p.Municipio,
            FechaCreacion = p.FechaCreacion
        };
    }

    public async Task AgregarAsync(Core.Entities.Persona domainPersona)
    {
        var efPersona = new Models.Persona
        {
            IdTipoDocumento = domainPersona.IdTipoDocumento,
            NumeroDocumento = domainPersona.NumeroDocumento,
            Nombres = domainPersona.Nombres,
            Apellidos = domainPersona.Apellidos,
            Telefono = domainPersona.Telefono,
            FechaNacimiento = domainPersona.FechaNacimiento,
            Direccion = domainPersona.Direccion,
            Municipio = domainPersona.Municipio,
            FechaCreacion = domainPersona.FechaCreacion
        };
        await context.Persona.AddAsync(efPersona);
        await context.SaveChangesAsync();
        domainPersona.IdPersona = efPersona.IdPersona;
    }
}
