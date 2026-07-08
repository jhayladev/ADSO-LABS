namespace AdsoLabs.Application.DTOs.Importacion;

/// <summary>
/// Resultado devuelto por el importador de aprendices tras procesar el archivo Excel.
///
/// Registra las operaciones realizadas sobre Persona, AprendizPerfil y FichaAprendiz,
/// con trazabilidad de errores por número de fila.
/// </summary>
public class ResultadoImportacionAprendizDto
{
    /// <summary>Total de filas de datos leídas (sin contar la fila de encabezados).</summary>
    public int TotalFilas { get; set; }

    /// <summary>Filas que fueron procesadas exitosamente.</summary>
    public int FilasOk { get; set; }

    /// <summary>Filas que generaron algún error.</summary>
    public int FilasError { get; set; }

    /// <summary>Aprendices (Persona + AprendizPerfil) nuevos creados en el sistema.</summary>
    public int AprendicesCreados { get; set; }

    /// <summary>
    /// Aprendices que ya existían en el sistema (datos actualizados si difieren).
    /// </summary>
    public int AprendicesActualizados { get; set; }

    /// <summary>Vínculos FichaAprendiz nuevos creados para la ficha importada.</summary>
    public int VinculosCreados { get; set; }

    /// <summary>Vínculos FichaAprendiz existentes cuyo estado fue actualizado.</summary>
    public int VinculosActualizados { get; set; }

    /// <summary>
    /// Mensajes de error con número de fila para trazabilidad.
    /// Un error en una fila no detiene el procesamiento de las demás.
    /// </summary>
    public List<string> Errores { get; set; } = [];
}
