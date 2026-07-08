using AdsoLabs.Application.DTOs.Informe;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IInformeRepository
{
    Task<AcudienteDTO?> ObtenerAcudienteAsync(int idAprendiz);
    Task GuardarAcudienteAsync(int idAprendiz, GuardarAcudienteDTO dto);
    Task<List<ObservacionAprendizDTO>> ObtenerObservacionesAsync(int idAprendiz);
    Task AgregarObservacionAsync(int idAprendiz, AgregarObservacionDTO dto);
    Task<List<CompetenciaSinCalificarDTO>> ObtenerCompetenciasSinCalificarAsync(int idAprendiz, string numeroFicha);
}
