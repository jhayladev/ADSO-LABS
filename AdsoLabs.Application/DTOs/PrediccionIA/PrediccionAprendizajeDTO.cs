namespace AdsoLabs.Application.DTOs.PrediccionIA;

public class CompetenciaAsignadaDTO
{
    public int IdFichaCompetencia { get; set; }
    public string NombreCompetencia { get; set; } = null!;
    public string CodigoCompetencia { get; set; } = null!;
    public string NumeroFicha { get; set; } = null!;
    public List<string> Resultados { get; set; } = [];
}

public class PrediccionAprendizajeDTO
{
    public string TendenciaGeneral { get; set; } = null!;
    public string DescripcionTendencia { get; set; } = null!;
    public List<TematicaDTO> Tematicas { get; set; } = [];
    public List<PasoRutaDTO> RutaAprendizaje { get; set; } = [];
    public List<KpiPrediccionDTO> Kpis { get; set; } = [];
    public DateTime GeneradoEn { get; set; }
}

public class TematicaDTO
{
    public int Numero { get; set; }
    public string Icono { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Relevancia { get; set; } = null!;  // Alta / Media / Baja
}

public class PasoRutaDTO
{
    public int Num { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
}

public class KpiPrediccionDTO
{
    public string Etiqueta { get; set; } = null!;
    public string Valor { get; set; } = null!;
    public string Icono { get; set; } = null!;
    public string Color { get; set; } = null!;  // green | blue | amber | violet
}
