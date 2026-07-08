using AdsoLabs.Application.DTOs.Instructor;

namespace AdsoLabs.Application.DTOs.Competencia;

public class FichaCompetenciaDTO
{
    public string NombreCompetencia { get; set; } = string.Empty;
    public string? CodigoCompetencia { get; set; }
    public int TotalCompetencias { get; set; } = 0;
    public int CompetenciasActivas { get; set; } = 0;
    public int CompetenciasClausuradas { get; set; } = 0;
    public List<FichaCompetenciaItemDTO> Fichas { get; set; } = new();
    public List<InstructorListadoDTO> Instructores { get; set; } = new();
}
