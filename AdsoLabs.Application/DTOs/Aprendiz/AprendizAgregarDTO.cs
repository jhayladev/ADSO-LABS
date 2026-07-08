namespace AdsoLabs.Application.DTOs.Aprendiz;

public class AprendizAgregarDTO
{
    //-- Datos Personales -----------
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;

    //-- Datos Aprendiz ------------
    public string NumeroFicha { get; set; } = string.Empty;
    public byte Estrato { get; set; }
    public string CondicionEspecial { get; set; } = string.Empty;
    public string TipoPoblacion { get; set; } = string.Empty;
    public string ContactoEmergenciaNombre { get; set; } = string.Empty;
    public string ContactoEmergenciaTelefono { get; set; } = string.Empty;
}
