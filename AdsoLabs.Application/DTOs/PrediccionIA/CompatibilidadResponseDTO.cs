namespace AdsoLabs.Application.DTOs.PrediccionIA;

public class CompatibilidadResponseDTO
{
    public string NombreInstructor { get; set; } = null!;
    public string Especialidad { get; set; } = null!;
    public int PorcentajeGeneral { get; set; }
    public string ResumenIA { get; set; } = null!;
    public List<SugerenciaDTO> Sugerencias { get; set; } = [];
    public List<string> FortalezasDetectadas { get; set; } = [];
}

public class SugerenciaDTO
{
    public int IdFichaCompetencia { get; set; }
    public string NombreCompetencia { get; set; } = null!;
    public string NumeroFicha { get; set; } = null!;
    public string Fase { get; set; } = null!;     // Lectiva / Productiva
    public string? FaseFormativa { get; set; }    // Induccion|Analisis|Planeacion|Ejecucion|Evaluacion|Transversal (de la competencia sugerida)
    public string FaseActualFicha { get; set; } = "";  // Fase en la que va la ficha actualmente (nombre legible)
    public int ProgresoFicha { get; set; }
    public int Compatibilidad { get; set; }
    public string Razonamiento { get; set; } = null!;
    public bool YaTieneSolicitudPendiente { get; set; }
}
