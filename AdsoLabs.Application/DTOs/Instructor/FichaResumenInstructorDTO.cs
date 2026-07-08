namespace AdsoLabs.Application.DTOs.Instructor;

public class FichaResumenInstructorDTO
{
    public string NumeroFicha { get; set; } = string.Empty;
    public string Programa    { get; set; } = string.Empty;
    public string? Jornada    { get; set; }
    public string? Modalidad  { get; set; }
    public bool   Activa      { get; set; }
    public int    Progreso    { get; set; }
}
