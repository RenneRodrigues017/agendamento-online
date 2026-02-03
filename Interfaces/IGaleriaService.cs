using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Barbearia.Models;

namespace Barbearia.Interfaces
{
   public interface IGaleriaService
    {
        Task SalvarFotosAsync(List<IFormFile> fotos, Guid tenantId);
        Task<List<GaleriaFoto>> ObterFotosAsync(Guid tenantId);
        Task ExcluirFotoAsync(Guid fotoId, Guid tenantId);
    }

}