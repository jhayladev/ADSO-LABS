using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IHashService
    {
        byte[] GenerarSalt();
        byte[] HashearPasswordSalt(string password, byte[] salt);
        byte[] HashearToken(string token);
    }
