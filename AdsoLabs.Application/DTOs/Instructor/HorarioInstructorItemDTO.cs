namespace AdsoLabs.Application.DTOs.Instructor;

public class HorarioInstructorItemDTO
{
    public string NumeroFicha       { get; set; } = string.Empty;
    public string NombreCompetencia { get; set; } = string.Empty;
    public string CodigoResultado   { get; set; } = string.Empty;
    public string NombreResultado   { get; set; } = string.Empty;
    public DateOnly? FechaInicio    { get; set; }
    public DateOnly? FechaFin       { get; set; }
    public TimeOnly? HoraInicio     { get; set; }
    public TimeOnly? HoraFin        { get; set; }
    public string Estado            { get; set; } = string.Empty;
}
