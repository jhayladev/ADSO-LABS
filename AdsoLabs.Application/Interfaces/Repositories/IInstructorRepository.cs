using AdsoLabs.Application.Commands.Instructor;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IInstructorRepository
{
    Task<ResultadoAgregar> AgregarAsync(AgregarInstructorCommand command);
    Task<bool> EditarAsync(int idInstructor);
}
