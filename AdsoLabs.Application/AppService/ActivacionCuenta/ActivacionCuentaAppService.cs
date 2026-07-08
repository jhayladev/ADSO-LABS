using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.AppService.ActivacionCuenta;

public class ActivacionCuentaAppService(IActivacionCuentaService activacionCuentaService)
{
    public Task<ResultadoActivacion> EnviarTokenActivacionAsync(string numeroDocumento, string correo)
        => activacionCuentaService.EnviarTokenActivacionAsync(numeroDocumento, correo);

    public Task<bool> ValidarTokenAsync(string token)
        => activacionCuentaService.ValidarTokenAsync(token);

    public Task<ResultadoActivacion> ActivarCuentaAsync(string token)
        => activacionCuentaService.ActivarCuentaAsync(token);
}
