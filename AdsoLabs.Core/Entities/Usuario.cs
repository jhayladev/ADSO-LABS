namespace AdsoLabs.Core.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdPersona { get; set; }
    public int IdRol { get; set; }
    public string Correo { get; set; } = null!;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public bool Activo { get; set; }
    public bool PrimerLogin { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaInicioSesion { get; set; }
    public DateTime? ConsentimientoFecha { get; set; }

    // Domain references (populated by repositories when needed)
    public Rol? Rol { get; set; }
    public Persona? Persona { get; set; }
}
