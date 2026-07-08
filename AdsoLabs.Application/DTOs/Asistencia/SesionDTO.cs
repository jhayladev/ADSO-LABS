namespace AdsoLabs.Application.DTOs.Asistencia;

public class SesionDTO
{
    public int      IdSesion        { get; set; }
    public DateOnly Fecha           { get; set; }
    public TimeOnly HoraInicio      { get; set; }
    public TimeOnly HoraFin         { get; set; }
    public string   Estado          { get; set; } = ""; // Abierta | Finalizada | Cancelada
    public int      TotalAprendices { get; set; }

    // Conteos por estado de asistencia
    public int Presentes    { get; set; }
    public int Tardes       { get; set; }
    public int Ausentes     { get; set; }
    public int Justificados { get; set; }

    // Calculados
    public int Asistentes   => Presentes + Tardes;
    public int NoAsistentes => Ausentes  + Justificados;
    public int SinRegistro  => TotalAprendices - Asistentes - NoAsistentes;
}
