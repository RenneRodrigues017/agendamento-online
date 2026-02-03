using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barbearia.Models
{
    public class Cliente 
    {
    public Guid Id { get; set; }
    
    // O TenantId vincula o cliente à barbearia específica
    public Guid TenantId { get; set; } 

    public string? Nome { get; set; }
    
    public string? Celular { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}