namespace AdsoLabs.Application.DTOs.Instructor;

public class AvisoInstructorDTO
{
    public string Tipo        { get; set; } = string.Empty; // SinCalificar | AprendizRiesgo | FichaVence
    public string Titulo      { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
