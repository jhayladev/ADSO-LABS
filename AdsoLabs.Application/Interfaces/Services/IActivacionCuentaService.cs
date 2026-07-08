using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IActivacionCuentaService
{
    /// <summary>
    /// El aprendiz proporciona su número de documento y correo.
    /// El sistema crea el usuario y envía el token de activación.
    /// </summary>
    Task<ResultadoActivacion> EnviarTokenActivacionAsync(string numeroDocumento, string correo);

    /// <summary>
    /// Valida que el token sea válido y no haya expirado.
    /// </summary>
    Task<bool> ValidarTokenAsync(string token);

    /// <summary>
    /// Establece la contraseña, activa el usuario y lo vincula al AprendizPerfil.
    /// </summary>
    Task<ResultadoActivacion> ActivarCuentaAsync(string token);
}
