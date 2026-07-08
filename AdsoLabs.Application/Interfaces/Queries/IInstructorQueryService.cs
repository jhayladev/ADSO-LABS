using AdsoLabs.Application.DTOs.Instructor;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IInstructorQueryService
{
    Task<List<InstructorListadoDTO>> ObtenerTodosAsync();
    Task<InstructorDetalleDTO?> ObtenerDetallesAsync(int idInstructor);
    Task<List<InstructorListadoDTO>> FiltrarAsync(string? nombreCompetencia, string? nombreInstructor);
    Task<List<InstructorListadoDTO>> ObtenerPorCompetenciaAsync(string nombreCompetencia);
    Task<ResumenInstructorDto> ObtenerResumenAsync(string? nombreCompetencia, string? nombreInstructor);
    Task<List<AvisoInstructorDTO>> ObtenerAvisosAsync(int idInstructor);
    Task<List<HorarioInstructorItemDTO>> ObtenerHorarioAsync(int idInstructor);
}
