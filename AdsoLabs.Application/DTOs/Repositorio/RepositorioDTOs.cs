namespace AdsoLabs.Application.DTOs.Repositorio;

public class DocumentoListadoDTO
{
    public int IdDocumento { get; set; }
    public string TipoDocumento { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = null!;
    public int Version { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int IdInstructor { get; set; }
    public string InstructorNombre { get; set; } = null!;
    public string? EntidadRelacionada { get; set; }
    public int IdArchivo { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string? ExtensionArchivo { get; set; }
    public long? TamanioBytes { get; set; }
}

public class DescargaDTO
{
    public long IdAuditoria { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string? ExtensionArchivo { get; set; }
    public DateTime FechaDescarga { get; set; }
    public int IdArchivo { get; set; }
}

public class ArchivoInfoDTO
{
    public int IdArchivo { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string RutaStorage { get; set; } = null!;
    public string? MimeType { get; set; }
    public string? ExtensionArchivo { get; set; }
    public int IdUsuarioSubio { get; set; }
}

public class PlanParaSubirDTO
{
    public int IdPlan { get; set; }
    public string CompetenciaNombre { get; set; } = null!;
    public string Estado { get; set; } = null!;
}

public class FichaCompetenciaSelectDTO
{
    public int IdFichaCompetencia { get; set; }
    public string NumeroFicha { get; set; } = null!;
    public string CompetenciaNombre { get; set; } = null!;
}

public class CompetenciaSelectDTO
{
    public int IdCompetencia { get; set; }
    public string Nombre { get; set; } = null!;
}

public class FichaSelectDTO
{
    public int IdFicha { get; set; }
    public string NumeroFicha { get; set; } = null!;
}

public class ResultadoOperacionRepositorio
{
    public bool Exito { get; set; }
    public string? Error { get; set; }
    public int? IdCreado { get; set; }

    public static ResultadoOperacionRepositorio Ok(int idCreado) => new() { Exito = true, IdCreado = idCreado };
    public static ResultadoOperacionRepositorio Fallo(string error) => new() { Exito = false, Error = error };
}
