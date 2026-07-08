using AdsoLabs.Application.DTOs.Auth;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;
using System.Security.Cryptography;

namespace AdsoLabs.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IHashService _hash;
        private const int IntentosMaximos = 5;
        private const int MinutosBloqueo = 15;
        private const string RolAdministrador = "Administrador";

        public AuthService(IUsuarioRepository usuarioRepo, IHashService hash)
        {
            _usuarioRepo = usuarioRepo;
            _hash = hash;
        }

        public async Task<LoginResultadoDto> ValidarCredencialesAsync(LoginPeticionDto peticion)
        {
            var usuario = await _usuarioRepo.ObtenerPorCorreoAsync(peticion.Correo);

            if (usuario == null || !usuario.Activo)
                return new LoginResultadoDto { esExitoso = false, Mensaje = "Usuario o contraseña incorrectos" };

            bool esAdministrador = usuario.Rol?.Nombre == RolAdministrador;

            if (!esAdministrador && usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta < DateTime.Now)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
                await _usuarioRepo.ActualizarLoginAsync(usuario.IdUsuario, 0, null, null);
            }

            if (!esAdministrador && usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.Now)
                return new LoginResultadoDto
                {
                    esExitoso = false,
                    Mensaje = $"Cuenta bloqueada. Intente de nuevo después de {usuario.BloqueadoHasta:HH:mm}."
                };

            bool passwordCorrecto;
            if (usuario.PrimerLogin)
            {
                var docEnBD = usuario.Persona?.NumeroDocumento?.Trim();
                passwordCorrecto = !string.IsNullOrEmpty(docEnBD)
                    && peticion.Password.Trim() == docEnBD;
            }
            else
                passwordCorrecto = VerificarPassword(peticion.Password, usuario.PasswordHash, usuario.PasswordSalt);

            if (!passwordCorrecto)
            {
                if (!esAdministrador)
                {
                    var nuevoIntentos = usuario.IntentosFallidos + 1;
                    DateTime? bloqueadoHasta = nuevoIntentos >= IntentosMaximos
                        ? DateTime.Now.AddMinutes(MinutosBloqueo)
                        : null;
                    await _usuarioRepo.ActualizarLoginAsync(usuario.IdUsuario, nuevoIntentos, bloqueadoHasta, null);
                }

                return new LoginResultadoDto
                {
                    esExitoso = false,
                    Mensaje = usuario.BloqueadoHasta.HasValue
                        ? $"Cuenta bloqueada por {MinutosBloqueo} minutos."
                        : "Usuario o contraseña incorrectos."
                };
            }

            await _usuarioRepo.ActualizarLoginAsync(usuario.IdUsuario, 0, null, DateTime.Now);

            int? idInstructor = usuario.Rol?.Nombre is "Instructor" or "Administrador"
                ? await _usuarioRepo.ObtenerIdInstructorPorUsuarioAsync(usuario.IdUsuario)
                : null;

            return new LoginResultadoDto
            {
                esExitoso = true,
                Mensaje = "Login exitoso.",
                Usuario = new UsuarioSesionDTO
                {
                    IdUsuario    = usuario.IdUsuario,
                    NombreRol    = usuario.Rol?.Nombre ?? string.Empty,
                    Nombres      = usuario.Persona?.Nombres ?? string.Empty,
                    Apellidos    = usuario.Persona?.Apellidos ?? string.Empty,
                    Correo       = usuario.Correo,
                    PrimerLogin  = usuario.PrimerLogin,
                    IdInstructor = idInstructor ?? 0
                }
            };
        }

        public bool VerificarPassword(string password, byte[]? hash, byte[]? salt)
        {
            if (hash == null || salt == null) return false;
            byte[] hashCalculado = _hash.HashearPasswordSalt(password, salt);
            return CryptographicOperations.FixedTimeEquals(hashCalculado, hash);
        }

        public async Task<bool> CambiarPasswordPrimerLoginAsync(int idUsuario, string nuevaPassword, bool consentimientoAceptado)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(idUsuario);
            if (usuario == null || !usuario.Activo || !usuario.PrimerLogin || !consentimientoAceptado)
                return false;

            byte[] salt = _hash.GenerarSalt();
            byte[] hash = _hash.HashearPasswordSalt(nuevaPassword, salt);
            await _usuarioRepo.CambiarPasswordAsync(idUsuario, hash, salt, false, DateTime.UtcNow);
            return true;
        }
    }
}
