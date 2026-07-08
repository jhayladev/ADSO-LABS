using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Core.Entities;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class FichaRepository : IFichaRepository
{
    private readonly AdsoDbContext _context;

    public FichaRepository(AdsoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ficha>> ObtenerFichasActivasAsync(int idUsuario = 0)
    {
        var query = _context.Ficha
            .Where(f => f.Estado == EstadosFicha.Activa);

        if (idUsuario > 0)
        {
            // Los administradores siempre ven todas las fichas, incluso si tienen
            // InstructorPerfil asignado. El filtro solo aplica al rol Instructor.
            bool esAdmin = await _context.Usuario
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => u.Rol.Nombre == "Administrador")
                .FirstOrDefaultAsync();

            if (!esAdmin)
            {
                var idInstructor = await _context.InstructorPerfil
                    .Where(ip => ip.IdUsuario == idUsuario)
                    .Select(ip => (int?)ip.IdInstructor)
                    .FirstOrDefaultAsync();

                if (idInstructor.HasValue)
                {
                    var idFichas = await _context.FichaCompetenciaResultado
                        .Where(fcr => fcr.IdInstructor == idInstructor.Value)
                        .Select(fcr => fcr.FichaCompetencia.IdFicha)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(f => idFichas.Contains(f.IdFicha));
                }
            }
        }

        return await query
            .Select(f => new Ficha
            {
                IdFicha = f.IdFicha,
                NumeroFicha = f.NumeroFicha,
                Nombre = f.Nombre,
                Descripcion = f.Descripcion,
                FechaInicio = f.FechaInicio,
                FechaFin = f.FechaFin,
                Estado = f.Estado,
                FechaCreacion = f.FechaCreacion
            })
            .ToListAsync();
    }

    
}
