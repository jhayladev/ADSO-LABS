namespace AdsoLabs.Infrastructure.Models;

public class AprendizPerfil
{
    public int IdAprendiz { get; set; }

    // IdPersona directo: el aprendiz existe desde la importación,
    // aunque todavía no tenga usuario creado en el sistema.
    public int IdPersona { get; set; }

    // Nullable: se asigna cuando el aprendiz activa su cuenta.
    public int? IdUsuario { get; set; }

    public byte? Estrato { get; set; }
    public string? CondicionEspecial { get; set; }
    public string? TipoPoblacion { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Navegación
    public Persona Persona { get; set; } = null!;
    public Usuario? Usuario { get; set; }
    public ICollection<Ficha> Fichas { get; set; } = [];              // skip navigation
    public ICollection<FichaAprendiz> FichaAprendices { get; set; } = []; // explicit join
    public ICollection<Asistencia> Asistencias { get; set; } = [];
    public ICollection<JuicioResultado> JuiciosResultado { get; set; } = [];
    public ICollection<Patrocinio> Patrocinios { get; set; } = [];
}
