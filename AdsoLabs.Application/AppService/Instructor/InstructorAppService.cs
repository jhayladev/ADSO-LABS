using AdsoLabs.Application.Commands.Instructor;
using AdsoLabs.Application.DTOs.Instructor;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.AppService.Instructor;

public class InstructorAppService(IInstructorRepository instructorRepo, IInstructorQueryService instructorQuery, IUsuarioRepository usuarioRepo)
{
    // Commands → IInstructorRepository
    public Task<ResultadoAgregar> AgregarInstructorAsync(InstructorAgregarDTO dto)
        => instructorRepo.AgregarAsync(new AgregarInstructorCommand(
            dto.TipoDocumento,
            dto.NumeroDocumento,
            dto.Nombre,
            dto.Apellido,
            dto.Telefono,
            dto.FechaNacimiento,
            dto.Direccion,
            dto.Municipio,
            dto.Correo,
            dto.NumeroContrato,
            dto.Especialidad,
            dto.TipoVinculacion,
            dto.FechaVinculacion
        ));

    public Task<bool> EditarInstructorAsync(int idInstructor)
        => instructorRepo.EditarAsync(idInstructor);

    public Task<bool> ActualizarContactoAsync(ActualizarContactoInstructorDTO dto)
        => usuarioRepo.ActualizarContactoAsync(dto.IdUsuario, dto.Correo, dto.Telefono);

    // Queries → IInstructorQueryService
    public Task<List<InstructorListadoDTO>> ObtenerInstructoresAsync()
        => instructorQuery.ObtenerTodosAsync();

    public Task<ResumenInstructorDto> ObtenerResumenInstructoresAsync(string? nombreCompetencia = null, string? nombreInstructor = null)
        => instructorQuery.ObtenerResumenAsync(nombreCompetencia, nombreInstructor);

    public Task<InstructorDetalleDTO?> ObtenerDetallesInstructorAsync(int idInstructor)
        => instructorQuery.ObtenerDetallesAsync(idInstructor);

    public Task<List<InstructorListadoDTO>> ObtenerInstructoresPorCompetenciaAsync(string nombreCompetencia)
        => instructorQuery.ObtenerPorCompetenciaAsync(nombreCompetencia);

    public Task<List<InstructorListadoDTO>> FiltrarInstructorAsync(string? nombreCompetencia = null, string? nombreInstructor = null)
        => instructorQuery.FiltrarAsync(nombreCompetencia, nombreInstructor);

    public Task<List<AvisoInstructorDTO>> ObtenerAvisosInstructorAsync(int idInstructor)
        => instructorQuery.ObtenerAvisosAsync(idInstructor);

    public Task<List<HorarioInstructorItemDTO>> ObtenerHorarioInstructorAsync(int idInstructor)
        => instructorQuery.ObtenerHorarioAsync(idInstructor);
}
