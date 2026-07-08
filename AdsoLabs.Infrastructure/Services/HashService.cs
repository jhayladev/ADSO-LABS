using AdsoLabs.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdsoLabs.Infrastructure.Services
{
    public class HashService : IHashService
    {

        private const int Iteraciones = 100_000;
        private const int TamanioHash = 32;

        public byte[] GenerarSalt()
        {
            return RandomNumberGenerator.GetBytes(16);
        }

        public byte[] HashearPasswordSalt(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iteraciones,
                HashAlgorithmName.SHA256
            );
            return pbkdf2.GetBytes(TamanioHash);
        }

        public byte[] HashearToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            return SHA256.HashData(bytes);
        }

    }
}
