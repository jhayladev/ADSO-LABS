using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Application.DTOs.Fichas;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;

namespace AdsoLabs.Application.AppService.Fichas;

public class FichaAppService(IFichaRepository repo, IFichaQueryService query, IAprendizQueryService aprendizQuery)
{
    public async Task<List<FichaDTO>> ObtenerFichasAsync(int idUsuario = 0)
    {
        var fichas = await repo.ObtenerFichasActivasAsync(idUsuario);
        return fichas.Select(f => new FichaDTO
        {
            IdFicha = f.IdFicha,
            NumeroFicha = f.NumeroFicha,
            Nombre = f.Nombre,
            Descripcion = f.Descripcion,
            FechaInicio = f.FechaInicio ?? DateOnly.MinValue,
            FechaFin = f.FechaFin,
            Estado = f.Estado,
            FechaCreacion = f.FechaCreacion
        }).ToList();
    }

    public Task<FichaDetallesDTO?> ObtenerDatosAsync(string numeroFicha) =>
        query.ObtenerDatosAsync(numeroFicha);

    public Task<List<CompetenciaFichaDTO>> ObtenerFichaCompetenciaAsync(string numeroFicha, int idUsuario = 0) =>
        query.ObtenerFichaCompetenciaAsync(numeroFicha, idUsuario);

    public Task<List<AprendizFichaDTO>> ObtenerAprendicesAsync(string numeroFicha) =>
        query.ObtenerAprendicesAsync(numeroFicha);
}