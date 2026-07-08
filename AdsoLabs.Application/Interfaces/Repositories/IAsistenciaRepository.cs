using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Asistencia;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IAsistenciaRepository
{
    /// <summary>
    /// Crea una sesión real para el resultado programado indicado.
    /// Valida que la fecha esté dentro del rango y que no exista duplicado.
    /// Devuelve el IdSesion creado dentro del DTO en caso de éxito.
    /// </summary>
    Task<MensajeEstadoDTO> CrearSesionAsync(CrearSesionDTO dto);

    /// <summary>
    /// Crea o actualiza la asistencia de cada aprendiz en la sesión indicada.
    /// </summary>
    Task<MensajeEstadoDTO> GuardarAsistenciaAsync(GuardarAsistenciaDTO dto);
}
