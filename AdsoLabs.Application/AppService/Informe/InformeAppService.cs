using AdsoLabs.Application.DTOs.Informe;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Informe;

public class InformeAppService(
    IInformeRepository informeRepo,
    INotificacionService notificacionService)
{
    public Task<AcudienteDTO?> ObtenerAcudienteAsync(int idAprendiz)
        => informeRepo.ObtenerAcudienteAsync(idAprendiz);

    public async Task GuardarAcudienteAsync(int idAprendiz, GuardarAcudienteDTO dto)
    {
        await informeRepo.GuardarAcudienteAsync(idAprendiz, dto);
        await notificacionService.NotificarDatosPersonalesActualizadosAsync(idAprendiz);
    }

    public Task<List<ObservacionAprendizDTO>> ObtenerObservacionesAsync(int idAprendiz)
        => informeRepo.ObtenerObservacionesAsync(idAprendiz);

    public async Task AgregarObservacionAsync(int idAprendiz, AgregarObservacionDTO dto)
    {
        await informeRepo.AgregarObservacionAsync(idAprendiz, dto);
        await notificacionService.NotificarObservacionAgregadaAsync(idAprendiz, dto.IdUsuario);
    }

    public Task<List<CompetenciaSinCalificarDTO>> ObtenerCompetenciasSinCalificarAsync(int idAprendiz, string numeroFicha)
        => informeRepo.ObtenerCompetenciasSinCalificarAsync(idAprendiz, numeroFicha);
}
