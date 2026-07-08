namespace AdsoLabs.Application.DTOs.Importacion;

/// <summary>
/// Resultado devuelto por el importador de fichas tras procesar el archivo Excel.
///
/// A diferencia de <see cref="ResultadoImportacionDto"/> (juicios SOFIA),
/// este DTO refleja operaciones a nivel de Ficha y FichaCompetencia.
/// </summary>
public class ResultadoImportacionFichaDto
{
    /// <summary>Total de filas de datos leídas (sin contar la fila de encabezados).</summary>
    public int TotalFilas { get; set; }

    /// <summary>Filas que fueron procesadas exitosamente (competencia vinculada o ficha actualizada).</summary>
    public int FilasOk { get; set; }

    /// <summary>Filas que generaron algún error (dato inválido, competencia inexistente, etc.).</summary>
    public int FilasError { get; set; }

    /// <summary>Fichas que no existían en el sistema y fueron creadas.</summary>
    public int FichasCreadas { get; set; }

    /// <summary>Fichas que ya existían y fueron actualizadas (Nombre, FechaInicio, FechaFin).</summary>
    public int FichasActualizadas { get; set; }

    /// <summary>Vínculos FichaCompetencia nuevos creados durante la importación.</summary>
    public int CompetenciasVinculadas { get; set; }

    /// <summary>Números de ficha afectados (creados o actualizados) por la importación.</summary>
    public List<string> FichasAfectadas { get; set; } = [];

    /// <summary>
    /// Mensajes de error con número de fila para trazabilidad.
    /// Cada fila fallida añade un mensaje aquí; las demás filas se siguen procesando.
    /// </summary>
    public List<string> Errores { get; set; } = [];
}
