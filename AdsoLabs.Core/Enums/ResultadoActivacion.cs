using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdsoLabs.Core.Enums
{
    public enum ResultadoActivacion
    {
        Exitoso,
        DocumentoNoEncontrado,
        YaActivado,
        CorreoEnUso,
        TokenInvalido,
        EnCooldown,
        AprendizInactivo
    }

}
