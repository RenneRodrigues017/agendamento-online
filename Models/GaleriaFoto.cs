using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barbearia.Models
{
    public class GaleriaFoto
        {
            public Guid Id { get; set; }

            public Guid TenantId { get; set; }

            public string ImagemPath { get; set; } = null!;

            public DateTime DataCadastro { get; set; }
        }

}