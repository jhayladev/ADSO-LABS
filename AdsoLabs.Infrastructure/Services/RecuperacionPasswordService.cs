using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Core.Enums;
using Microsoft.Extensions.Configuration;

namespace AdsoLabs.Infrastructure.Services
{
    public class RecuperacionPasswordService : IRecuperacionPasswordService
    {
        private readonly IEmailServices _email;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IConfiguration _config;
        private readonly IHashService _hash;

        private static readonly TimeSpan _cooldown = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan _expiracion = TimeSpan.FromMinutes(30);

        public RecuperacionPasswordService(
            IEmailServices email,
            IUsuarioRepository usuarioRepo,
            IPasswordResetTokenRepository tokenRepo,
            IConfiguration config,
            IHashService hash)
        {
            _email = email;
            _usuarioRepo = usuarioRepo;
            _tokenRepo = tokenRepo;
            _config = config;
            _hash = hash;
        }

        public async Task<ResultadoEnvioToken> EnviarTokenAsync(string correo)
        {
            var usuario = await _usuarioRepo.ObtenerPorCorreoAsync(correo);

            if (usuario == null)
                return ResultadoEnvioToken.Exitoso; // No revelar si el correo existe

            if (usuario.PrimerLogin)
                return ResultadoEnvioToken.EsPrimerLogin;

            var limite = DateTime.UtcNow.Subtract(_cooldown);
            bool enCooldown = await _tokenRepo.ExisteTokenEnCooldownAsync(usuario.IdUsuario, limite);
            if (enCooldown)
                return ResultadoEnvioToken.EnCooldown;

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
            var link = $"{baseUrl}/recuperacionhandler?token={token}";

            var htmlRecuperacion = $@"<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
  <title>Recuperación de contraseña — ADSO Labs</title>
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
                          <span style='display:inline-block;width:56px;height:56px;background:#FEF3C7;border-radius:50%;text-align:center;line-height:56px;font-size:24px;'>🔑</span>
                        </td>
                      </tr>
                    </table>

                    <!-- Heading -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td align='center' style='padding-bottom:8px;'>
                          <h1 style='margin:0;font-size:22px;font-weight:700;color:#1E293B;'>Recupera tu contraseña</h1>
                        </td>
                      </tr>
                      <tr>
                        <td align='center' style='padding-bottom:32px;'>
                          <p style='margin:0;font-size:14px;color:#64748B;line-height:1.65;'>
                            Recibimos una solicitud para restablecer la contraseña<br>
                            de tu cuenta en <strong style='color:#4F46E5;'>ADSO Labs</strong>.
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
                            Restablecer contraseña
                          </a>
                        </td>
                      </tr>
                    </table>

                    <!-- Info box -->
                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td style='background:#F8FAFC;border-radius:10px;border:1px solid #F1F5F9;padding:16px 20px;'>
                          <p style='margin:0;font-size:13px;color:#64748B;line-height:1.65;'>
                            <strong style='color:#1E293B;'>¿No solicitaste este cambio?</strong><br>
                            Si no pediste recuperar tu contraseña, puedes ignorar este correo. Tu contraseña actual seguirá siendo la misma.
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
                            🛡&nbsp; ADSO Labs nunca te pedirá tu contraseña por correo
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
                "Recuperación de contraseña — ADSO Labs",
                htmlRecuperacion);

            return ResultadoEnvioToken.Exitoso;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            var tokenHash = Convert.ToHexString(_hash.HashearToken(token));
            return await _tokenRepo.ExisteTokenValidoAsync(tokenHash);
        }

        public async Task<bool> CambiarPasswordAsync(string token, string nuevaPassword)
        {
            var tokenHash = Convert.ToHexString(_hash.HashearToken(token));
            var registro = await _tokenRepo.ObtenerPorHashValidoAsync(tokenHash);
            if (registro == null) return false;

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(registro.IdUsuario);
            if (usuario == null) return false;

            byte[] salt = _hash.GenerarSalt();
            byte[] hash = _hash.HashearPasswordSalt(nuevaPassword, salt);
            await _usuarioRepo.CambiarPasswordAsync(usuario.IdUsuario, hash, salt, false);

            await _tokenRepo.MarcarComoUsadoAsync(registro.IdToken);
            return true;
        }
    }
}
