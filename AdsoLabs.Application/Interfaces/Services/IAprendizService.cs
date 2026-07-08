namespace AdsoLabs.Application.Interfaces.Services;

/// <summary>
/// Contrato para operaciones de creación y vinculación de aprendices.
///
/// Este servicio centraliza la lógica de persistencia de Persona, AprendizPerfil
/// y FichaAprendiz para que pueda ser reutilizada desde distintos módulos
/// (importador de aprendices, actualización de estados, etc.) sin duplicar código.
///
/// El importador de juicios evaluativos de SOFIA NO usa este servicio para crear
/// registros: solo consulta si el aprendiz existe y actualiza su estado si ya está
/// vinculado a la ficha.
/// </summary>
public interface IAprendizService
{
    /// <summary>
    /// Crea o actualiza el registro de Persona identificado por su número de documento.
    /// Si ya existe, actualiza nombres y apellidos con los valores proporcionados.
    /// </summary>
    /// <param name="numeroDocumento">Número único de identificación del aprendiz.</param>
    /// <param name="nombres">Nombres del aprendiz tal como aparecen en la fuente.</param>
    /// <param name="apellidos">Apellidos del aprendiz.</param>
    /// <param name="idTipoDocumento">FK al catálogo TipoDocumento.</param>
    /// <returns>ID de la Persona creada o encontrada.</returns>
    Task<int> UpsertPersonaAsync(
        string numeroDocumento,
        string nombres,
        string apellidos,
        int    idTipoDocumento);

    /// <summary>
    /// Crea un AprendizPerfil vinculado a la Persona indicada.
    /// Si el perfil ya existe, lo devuelve sin modificar.
    /// </summary>
    /// <param name="idPersona">ID de la Persona ya persistida.</param>
    /// <returns>
    ///   idAprendiz: PK del AprendizPerfil.
    ///   isNew: true si se acaba de insertar, false si ya existía.
    /// </returns>
    Task<(int idAprendiz, bool isNew)> UpsertAprendizPerfilAsync(int idPersona);

    /// <summary>
    /// Vincula un aprendiz a una ficha o actualiza el estado del vínculo existente.
    ///
    /// Aplica la regla de traslado automático (CLAUDE.md §7.9):
    ///   • Al vincular como EN FORMACION, todas las fichas anteriores donde el aprendiz
    ///     estuviera activo se marcan como TRASLADADO.
    ///   • Si la BD indica TRASLADADO pero el Excel dice EN FORMACION y existe una ficha
    ///     más reciente, se conserva el traslado (no se revierte historia).
    /// </summary>
    /// <param name="idFicha">ID de la ficha destino.</param>
    /// <param name="idAprendiz">ID del AprendizPerfil.</param>
    /// <param name="estadoAprendiz">Estado canónico (EN FORMACION, CANCELADO, etc.).</param>
    Task VincularAprendizFichaAsync(int idFicha, int idAprendiz, string estadoAprendiz);
}
