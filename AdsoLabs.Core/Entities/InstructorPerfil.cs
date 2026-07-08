namespace AdsoLabs.Core.Entities;

public class InstructorPerfil
{
    public int IdInstructor { get; set; }
    public int IdUsuario { get; set; }
    public string? NumeroContrato { get; set; }
    public string? Especialidad { get; set; }
    public string? TipoVinculacion { get; set; }
    public DateOnly? FechaVinculacion { get; set; }
    public bool Activo { get; set; }
}
