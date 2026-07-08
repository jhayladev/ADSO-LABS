namespace AdsoLabs.Application.DTOs.Dashboard;

public class EstadoCompetenciasDashboardDTO
{
    public int Total { get; set; }

    /// <summary>Estado = Vista.</summary>
    public int Finalizadas { get; set; }

    /// <summary>Estado = En Curso o Programada (al menos un FCR programado).</summary>
    public int EnEjecucion { get; set; }

    /// <summary>
    /// No finalizadas donde HorasEjecutadas / TotalHoras &gt;= 0.80.
    /// </summary>
    public int ProximasACerrar { get; set; }

    /// <summary>Sin ningún FCR programado y estado Pendiente.</summary>
    public int Pendientes { get; set; }
}
