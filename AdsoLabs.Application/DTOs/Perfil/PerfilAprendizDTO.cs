namespace AdsoLabs.Application.DTOs.Perfil;

public class PerfilAprendizDTO
{
    public int IdAprendiz { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public byte? Estrato { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? CondicionEspecial { get; set; }
    public string? TipoPoblacion { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public List<FichaPerfilDTO> Fichas { get; set; } = [];
}

public class FichaPerfilDTO
{
    public string NumeroFicha { get; set; } = string.Empty;
    public string EstadoFicha { get; set; } = string.Empty;
    public string EstadoAprendizEnFicha { get; set; } = string.Empty;
    public List<CompetenciaPerfilDTO> Competencias { get; set; } = [];
}

public class CompetenciaPerfilDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int? TotalHoras { get; set; }
    public List<ResultadoPerfilDTO> Resultados { get; set; } = [];
}

public class ResultadoPerfilDTO
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string EstadoProgramacion { get; set; } = string.Empty; // Pendiente | Programado | Completado
    public string? NombreInstructor { get; set; }
    public string Juicio { get; set; } = string.Empty; // Aprobado | Por Evaluar | Sin evaluar
}
