namespace AdsoLabs.Application.DTOs.Dashboard;

public class ResumenDashboardDTO
{
    public string Nombres { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime? UltimaActividad { get; set; }
    public int FichasActivas { get; set; }
    public int TotalHorasPlaneadas { get; set; }

    /// <summary>
    /// Promedio ponderado de ProgresoGeneral de las fichas activas del instructor.
    /// </summary>
    public int ProgresoPromedio { get; set; }

    /// <summary>
    /// Número de instructores distintos (incluido el propio) que tienen FCRs
    /// asignados en las fichas del instructor autenticado.
    /// </summary>
    public int InstructoresTecnicos { get; set; }

    public List<EstadoFichaResumenDTO> Fichas { get; set; } = [];
}

public class EstadoFichaResumenDTO
{
    public string NumeroFicha { get; set; } = string.Empty;
    public int ProgresoGeneral { get; set; }

    /// <summary>
    /// Clasificación basada en ProgresoGeneral:
    /// Crítico (&lt;30%) · Riesgo (30–60%) · Excelencia (&gt;60%)
    /// </summary>
    public string ClasificacionEstado { get; set; } = string.Empty;
}
