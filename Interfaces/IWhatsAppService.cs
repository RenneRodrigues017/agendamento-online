using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barbearia.Interfaces
{
    public interface IWhatsAppService
    {
        Task EnviarMensagemAsync(string telefone, string mensagem);
    }
}