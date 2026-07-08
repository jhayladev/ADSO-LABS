using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Core.Enums;
using Microsoft.Extensions.Configuration;

namespace AdsoLabs.Infrastructure.Services
{
    public class ActivacionCuentaService : IActivacionCuentaService
    {
        private readonly IEmailServices _email;
        private readonly IPersonaRepository _personaRepo;
        private readonly IAprendizRepository _aprendizRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IRolRepository _rolRepo;
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IConfiguration _config;
        private readonly IHashService _hash;

        private static readonly TimeSpan _cooldown = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan _expiracion = TimeSpan.FromMinutes(30);

        public ActivacionCuentaService(
            IEmailServices email,
            IPersonaRepository personaRepo,
            IAprendizRepository aprendizRepo,
            IUsuarioRepository usuarioRepo,
            IRolRepository rolRepo,
            IPasswordResetTokenRepository tokenRepo,
            IConfiguration config,
            IHashService hash)
        {
            _email = email;
            _personaRepo = personaRepo;
            _aprendizRepo = aprendizRepo;
            _usuarioRepo = usuarioRepo;
            _rolRepo = rolRepo;
            _tokenRepo = tokenRepo;
            _config = config;
            _hash = hash;
        }

        public async Task<ResultadoActivacion> EnviarTokenActivacionAsync(string numeroDocumento, string correo)
        {
            var persona = await _personaRepo.ObtenerPorNumeroDocumentoAsync(numeroDocumento);
            if (persona == null)
                return ResultadoActivacion.DocumentoNoEncontrado;

            var perfil = await _aprendizRepo.ObtenerPorIdPersonaAsync(persona.IdPersona);
            if (perfil == null)
                return ResultadoActivacion.DocumentoNoEncontrado;

            if (perfil.IdUsuario != null)
                return ResultadoActivacion.YaActivado;

            var estadoActual = await _aprendizRepo.ObtenerEstadoMasRecienteAsync(perfil.IdAprendiz);
            if (estadoActual is "Cancelado" or "Retiro Voluntario")
                return ResultadoActivacion.AprendizInactivo;

            bool correoEnUso = await _usuarioRepo.ExisteCorreoAsync(correo);
            if (correoEnUso)
                return ResultadoActivacion.CorreoEnUso;

            var usuario = await _usuarioRepo.ObtenerPorIdPersonaAsync(persona.IdPersona);

            if (usuario == null)
            {
                var idRolAprendiz = await _rolRepo.ObtenerIdPorNombreAsync("Aprendiz");
                var nuevoUsuario = new Core.Entities.Usuario
                {
                    IdPersona = persona.IdPersona,
                    IdRol = idRolAprendiz,
                    Correo = correo,
                    Activo = false,
                    PrimerLogin = true,
                    IntentosFallidos = 0,
                    FechaCreacion = DateTime.UtcNow
                };
                await _usuarioRepo.AgregarAsync(nuevoUsuario);
                usuario = nuevoUsuario;
            }
            else
            {
                await _usuarioRepo.ActualizarCorreoAsync(usuario.IdUsuario, correo);
            }

            var limite = DateTime.UtcNow.Subtract(_cooldown);
            bool enCooldown = await _tokenRepo.ExisteTokenEnCooldownAsync(usuario.IdUsuario, limite);
            if (enCooldown)
                return ResultadoActivacion.EnCooldown;

            await _tokenRepo.InvalidarTokensPendientesAsync(usuario.IdUsuario);

            var token = Guid.NewGuid().ToString();
            var tokenHash = Convert.ToHexString(_hash.HashearToken(token));

            await _tokenRepo.AgregarAsync(new Core.Entities.PasswordResetToken
            {
                IdUsuario = usuario.IdUsuario,
                TokenHash = tokenHash,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.Add(_expiracion),
                FechaUso = null
            });

            var baseUrl = _config["App:BaseUrl"];
            var link = $"{baseUrl}/activacionhandler?token={token}";

            var htmlActivacion = $@"<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
  <title>Activa tu cuenta — ADSO Labs</title>
</head>
<body style='margin:0;padding:0;background-color:#F4F6FB;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Helvetica Neue,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#F4F6FB;padding:40px 16px;'>
    <tr>
      <td align='center'>
        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:560px;'>

          <!-- Logo -->
          <tr>
            <td style='padding-bottom:24px;'>
              <table cellpadding='0' cellspacing='0' border='0'>
                <tr>
                  <td style='background:linear-gradient(135deg,#4F46E5,#3B82F6);border-radius:10px;width:36px;height:36px;text-align:center;vertical-align:middle;'>
                    <span style='color:white;font-size:18px;font-weight:700;line-height:36px;display:block;'>A</span>
                  </td>
                  <td style='padding-left:10px;vertical-align:middle;'>
                    <span style='font-size:17px;font-weight:700;color:#1E293B;'>ADSO Labs</span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Card -->
          <tr>
            <td style='background:#ffffff;border-radius:16px;border:1px solid #E2E8F0;overflow:hidden;'>

              <!-- Accent bar -->
              <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr>
                  <td style='height:4px;background:linear-gradient(90deg,#4F46E5,#3B82F6);font-size:0;line-height:0;'>&nbsp;</td>
                </tr>
              </table>

              <!-- Body -->
              <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr>
                  <td style='padding:40px 44px 36px;'>

                    <!-- Icon -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td align='center' style='padding-bottom:24px;'>
                          <span style='display:inline-block;width:56px;height:56px;background:#EDE9FE;border-radius:50%;text-align:center;line-height:56px;font-size:24px;'>🔐</span>
                        </td>
                      </tr>
                    </table>

                    <!-- Heading -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td align='center' style='padding-bottom:8px;'>
                          <h1 style='margin:0;font-size:22px;font-weight:700;color:#1E293B;'>Activa tu cuenta</h1>
                        </td>
                      </tr>
                      <tr>
                        <td align='center' style='padding-bottom:32px;'>
                          <p style='margin:0;font-size:14px;color:#64748B;line-height:1.65;'>
                            Te damos la bienvenida a <strong style='color:#4F46E5;'>ADSO Labs</strong>,<br>
                            la plataforma de gestión académica del programa ADSO — SENA.
                          </p>
                        </td>
                      </tr>
                    </table>

                    <!-- CTA Button -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td align='center' style='padding-bottom:32px;'>
                          <a href='{link}'
                             style='display:inline-block;background:#4F46E5;color:#ffffff;text-decoration:none;padding:14px 44px;border-radius:10px;font-size:15px;font-weight:600;letter-spacing:0.01em;'>
                            Activar mi cuenta
                          </a>
                        </td>
                      </tr>
                    </table>

                    <!-- Info box -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td style='background:#F8FAFC;border-radius:10px;border:1px solid #F1F5F9;padding:16px 20px;margin-bottom:24px;'>
                          <p style='margin:0;font-size:13px;color:#64748B;line-height:1.65;'>
                            <strong style='color:#1E293B;'>¿Qué ocurre después?</strong><br>
                            Al hacer clic en el botón podrás establecer tu contraseña personal y acceder a la plataforma con tu número de documento.
                          </p>
                        </td>
                      </tr>
                    </table>

                    <!-- Security notes -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin-top:20px;margin-bottom:24px;'>
                      <tr>
                        <td style='border-left:3px solid #E2E8F0;padding-left:14px;'>
                          <p style='margin:0;font-size:12px;color:#94A3B8;line-height:1.8;'>
                            ⏱&nbsp; Este enlace expira en <strong style='color:#64748B;'>30 minutos</strong><br>
                            🔒&nbsp; Por seguridad, nunca compartas este enlace con nadie<br>
                            ✉&nbsp; Si no solicitaste esto, puedes ignorar este correo
                          </p>
                        </td>
                      </tr>
                    </table>

                    <!-- Fallback link -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td align='center'>
                          <p style='margin:0;font-size:11px;color:#94A3B8;line-height:1.6;'>
                            Si el botón no funciona, copia y pega este enlace en tu navegador:<br>
                            <span style='color:#4F46E5;word-break:break-all;'>{link}</span>
                          </p>
                        </td>
                      </tr>
                    </table>

                  </td>
                </tr>
              </table>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding:24px 0;text-align:center;'>
              <p style='margin:0;font-size:11px;color:#94A3B8;line-height:1.7;'>
                ADSO Labs &nbsp;·&nbsp; Servicio Nacional de Aprendizaje SENA<br>
                Este es un mensaje automático — no respondas a este correo.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            await _email.EnviarAsync(
                correo,
                "Activa tu cuenta en ADSO Labs",
                htmlActivacion);

            return ResultadoActivacion.Exitoso;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            var tokenHash = Convert.ToHexString(_hash.HashearToken(token));
            return await _tokenRepo.ExisteTokenValidoAsync(tokenHash);
        }

        public async Task<ResultadoActivacion> ActivarCuentaAsync(string token)
        {
            var tokenHash = Convert.ToHexString(_hash.HashearToken(token));
            var registro = await _tokenRepo.ObtenerPorHashValidoAsync(tokenHash);

            if (registro == null)
                return ResultadoActivacion.TokenInvalido;

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(registro.IdUsuario);
            if (usuario == null)
                return ResultadoActivacion.TokenInvalido;

            await _usuarioRepo.ActivarAsync(usuario.IdUsuario);

            var perfil = await _aprendizRepo.ObtenerPorIdPersonaAsync(usuario.IdPersona);
            if (perfil != null)
                await _aprendizRepo.AsignarUsuarioAsync(perfil.IdAprendiz, usuario.IdUsuario);

            await _tokenRepo.MarcarComoUsadoAsync(registro.IdToken);

            return ResultadoActivacion.Exitoso;
        }
    }
}
