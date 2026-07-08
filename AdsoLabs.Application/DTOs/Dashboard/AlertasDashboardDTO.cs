namespace AdsoLabs.Application.DTOs.Dashboard;

public class AlertasDashboardDTO
{
    /// <summary>
    /// Fichas donde la tasa de asistencia (Presente + Justificado / total)
    /// es inferior al 70% en al menos una sesión registrada.
    /// </summary>
    public List<AlertaFichaItemDTO> BajaAsistencia { get; set; } = [];

    /// <summary>
    /// Fichas donde ProgresoGeneral está más de 10 puntos por debajo del ProgresoTemporal.
    /// </summary>
    public List<AlertaFichaItemDTO> RetrasoHoras { get; set; } = [];

    /// <summary>
    /// Fichas donde ProgresoGeneral >= ProgresoTemporal - 10 (dentro del margen).
    /// </summary>
    public List<AlertaFichaItemDTO> AlDia { get; set; } = [];

    /// <summary>
    /// Fichas cuya FechaFin está dentro de los próximos 7 días calendario.
    /// </summary>
    public List<AlertaFichaItemDTO> CulminanEstaSemana { get; set; } = [];
}

public class AlertaFichaItemDTO
{
    public string NumeroFicha { get; set; } = string.Empty;
    public int ProgresoGeneral { get; set; }
    public int ProgresoTemporal { get; set; }
}
