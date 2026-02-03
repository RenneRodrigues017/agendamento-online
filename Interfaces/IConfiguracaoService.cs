using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barbearia.Interfaces
{
    public interface IConfiguracaoService
    {
        Task ConcluirAgendamentoAsync(Guid agendamentoId, Guid tenantId);
    }

}