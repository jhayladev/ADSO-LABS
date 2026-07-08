namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte F — Por instructor.
/// Detalle de carga académica de un instructor: fichas, competencias y horas.
/// </summary>
public class ReporteInstructorDTO
{
    public int    IdInstructor        { get; set; }
    public string NombreCompleto      { get; set; } = string.Empty;
    public string Correo              { get; set; } = string.Empty;
    public int    TotalFichas         { get; set; }
    public int    TotalCompetencias   { get; set; }
    public int    TotalHorasPlaneadas { get; set; }
    public int    TotalHorasEjecutadas{ get; set; }

    public List<ReporteInstructorFichaFila> Fichas { get; set; } = [];
}

public class ReporteInstructorFichaFila
{
    public string                                  NumeroFicha  { get; set; } = string.Empty;
    public List<ReporteInstructorCompetenciaFila>  Competencias { get; set; } = [];
}

public class ReporteInstructorCompetenciaFila
{
    public string CodigoCompetencia  { get; set; } = string.Empty;
    public string NombreCompetencia  { get; set; } = string.Empty;
    public string Estado             { get; set; } = string.Empty;
    public int    HorasPlaneadas     { get; set; }
    public int    HorasEjecutadas    { get; set; }
}

/// <summary>
/// Ítem del selector de instructores (solo admin).
/// </summary>
public class InstructorSelectorDTO
{
    public int    IdInstructor   { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}
