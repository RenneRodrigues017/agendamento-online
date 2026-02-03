using Barbearia.Models;

namespace Barbearia.Interfaces
{
    public interface ITenant
    {
        Guid Id { get; }
        string? Nome { get; set; }
    }
}
