namespace AdsoLabs.Infrastructure.Models;

public class GuiaAprendizaje
{
    public int IdGuia { get; set; }
    public int IdFichaCompetencia { get; set; }
    public int IdInstructor { get; set; }
    public int IdDocumento { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "Borrador"; // Borrador | Publicado | Cerrado
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }

    public FichaCompetencia FichaCompetencia { get; set; } = null!;
    public InstructorPerfil Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
}

public class InstrumentoEvaluacion
{
    public int IdInstrumento { get; set; }
    public int IdCompetencia { get; set; }
    public int IdInstructor { get; set; }
    public int IdDocumento { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "Borrador";
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }

    public Competencia Competencia { get; set; } = null!;
    public InstructorPerfil Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
}

public class PlaneacionPedagogica
{
    public int IdPlaneacion { get; set; }
    public int IdFichaCompetencia { get; set; }
    public int IdInstructor { get; set; }
    public int IdDocumento { get; set; }
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "Borrador";
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }

    public FichaCompetencia FichaCompetencia { get; set; } = null!;
    public InstructorPerfil Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
}

public class ProyectoFormativo
{
    public int IdProyecto { get; set; }
    public int IdFicha { get; set; }
    public int IdInstructor { get; set; }
    public int IdDocumento { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "Borrador";
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }

    public Ficha Ficha { get; set; } = null!;
    public InstructorPerfil Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
}

public class DesarrolloCurricular
{
    public int IdDesarrollo { get; set; }
    public int IdCompetencia { get; set; }
    public int IdInstructor { get; set; }
    public int IdDocumento { get; set; }
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "Borrador";
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }

    public Competencia Competencia { get; set; } = null!;
    public InstructorPerfil Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
}
