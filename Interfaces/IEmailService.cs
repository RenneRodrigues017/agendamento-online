using Barbearia.Models.ViewsModels;

namespace Barbearia.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailContatoAsync(ContatoViewModel dados);
    }
}
