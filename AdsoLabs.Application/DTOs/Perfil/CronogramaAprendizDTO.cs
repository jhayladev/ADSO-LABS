namespace AdsoLabs.Application.DTOs.Perfil;

public class CronogramaAprendizDTO
{
    public List<SesionCronogramaDTO> Sesiones { get; set; } = [];
    public PatrocinioCronogramaDTO? Patrocinio { get; set; }
}

public class SesionCronogramaDTO
{
    public string NumeroFicha { get; set; } = string.Empty;
    public string NombreCompetencia { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string NombreInstructor { get; set; } = string.Empty;
    public string EstadoSesion { get; set; } = string.Empty;
}

public class PatrocinioCronogramaDTO
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Etapa { get; set; } = string.Empty;
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public DateOnly? FechaInicioEtapa { get; set; }
    public DateOnly? FechaFinEtapa { get; set; }
}
