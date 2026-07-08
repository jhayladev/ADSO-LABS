namespace AdsoLabs.Application.DTOs.Instructor;

public class InstructorListadoDTO
{
    public int          IdInstructor    { get; set; }
    public string       Nombres         { get; set; } = string.Empty;
    public bool         Activo          { get; set; }
    public string?      Especialidad    { get; set; }
    public string?      TipoVinculacion { get; set; }
    public List<string> Competencias    { get; set; } = [];
    /// <summary>Fichas en las que este instructor participa en la competencia consultada.</summary>
    public List<string> Fichas          { get; set; } = [];
}
