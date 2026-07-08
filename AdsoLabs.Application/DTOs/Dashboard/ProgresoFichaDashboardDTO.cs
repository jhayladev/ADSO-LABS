namespace AdsoLabs.Application.DTOs.Dashboard;

public class ProgresoFichaDashboardDTO
{
    public string NumeroFicha { get; set; } = string.Empty;

    /// <summary>
    /// Suma de TotalHoras (o HorasAsignadas del catálogo si no configurado)
    /// de todas las FichaCompetencias de la ficha.
    /// </summary>
    public int HorasPlaneadas { get; set; }

    /// <summary>
    /// Suma de HorasEjecutadas de todas las FichaCompetencias de la ficha.
    /// </summary>
    public int HorasEjecutadas { get; set; }
}
