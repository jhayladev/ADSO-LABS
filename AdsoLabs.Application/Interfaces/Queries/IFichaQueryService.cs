using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Application.DTOs.Fichas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdsoLabs.Application.Interfaces.Queries
{
    public interface IFichaQueryService
    {

        Task<FichaDetallesDTO?> ObtenerDatosAsync(string numeroFicha);
        Task<List<CompetenciaFichaDTO>> ObtenerFichaCompetenciaAsync(string numeroFicha, int idUsuario = 0);
        Task<List<AprendizFichaDTO>> ObtenerAprendicesAsync(string numeroFicha);
        

    }
}
