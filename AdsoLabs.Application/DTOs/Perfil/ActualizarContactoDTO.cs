namespace AdsoLabs.Application.DTOs.Perfil;

public class ActualizarContactoDTO
{
    // Contacto (Persona)
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }

    // Datos personales (Persona + AprendizPerfil)
    public DateOnly? FechaNacimiento { get; set; }
    public byte? Estrato { get; set; }
    public string? TipoPoblacion { get; set; }
    public string? CondicionEspecial { get; set; }

    // Emergencia (AprendizPerfil)
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
}
