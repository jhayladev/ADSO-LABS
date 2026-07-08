using AdsoLabs.Application.Commands.Aprendiz;
using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.AppService.Aprendiz;

public class AprendizAppService(IAprendizRepository aprendizRepo, IAprendizQueryService aprendizQuery)
{
    private static readonly string[] EstadosValidos =
    [
        EstadosAprendiz.EnFormacion,
        EstadosAprendiz.Cancelado,
        EstadosAprendiz.RetiroVoluntario,
        EstadosAprendiz.Trasladado
    ];

    // Commands → IAprendizRepository
    public Task<ResultadoAgregar> AgregarAprendizAsync(AprendizAgregarDTO dto)
        => aprendizRepo.AgregarAsync(new AgregarAprendizCommand(
            dto.TipoDocumento,
            dto.NumeroDocumento,
            dto.Nombre,
            dto.Apellido,
            dto.Telefono,
            dto.FechaNacimiento,
            dto.Direccion,
            dto.Municipio,
            dto.NumeroFicha,
            dto.Estrato,
            dto.CondicionEspecial,
            dto.TipoPoblacion,
            dto.ContactoEmergenciaNombre,
            dto.ContactoEmergenciaTelefono
        ));

    public async Task<bool> EditarAprendizAsync(int idAprendiz, int idFicha, string estado, int? idUsuarioCambio = null)
    {
        // "Trasladado" solo lo establece la importación SOFIA Plus — nunca la UI
        if (estado == EstadosAprendiz.Trasladado) return false;
        if (!EstadosValidos.Contains(estado))     return false;

        // Verificar estado actual: si ya es Trasladado, es estado final e inmutable
        var estadoActual = await aprendizRepo.ObtenerEstadoActualAsync(idAprendiz, idFicha);
        if (estadoActual == EstadosAprendiz.Trasladado) return false;

        return await aprendizRepo.EditarEstadoAsync(idAprendiz, idFicha, estado, idUsuarioCambio);
    }

    // Queries → IAprendizQueryService
    public Task<List<AprendizListadoDTO>> ObtenerAprendicesAsync()
        => aprendizQuery.ObtenerTodosAsync();

    public Task<List<AprendizListadoDTO>> ObtenerAprendicesPorFichaAsync(string numeroFicha)
        => aprendizQuery.ObtenerPorFichaAsync(numeroFicha);

    public Task<AprendizDetalleDTO?> ObtenerDetallesAprendizAsync(int idAprendiz)
        => aprendizQuery.ObtenerDetallesAsync(idAprendiz);

    public Task<ResumenAprendizDto> ObtenerResumenAprendicesAsync(string? numeroFicha = null, string? numeroDocumento = null, string? nombreAprendiz = null)
        => aprendizQuery.ObtenerResumenAsync(numeroFicha, numeroDocumento, nombreAprendiz);

    public Task<List<AprendizListadoDTO>> FiltrarAprendizAsync(string? numeroFicha = null, string? numeroDocumento = null, string? nombreAprendiz = null)
        => aprendizQuery.FiltrarAsync(numeroFicha, numeroDocumento, nombreAprendiz);
}
