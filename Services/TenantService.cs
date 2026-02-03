using Barbearia.Interfaces;
using Barbearia.Models;
namespace Barbearia.Services
{
    public class TenantService : ITenant
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
    }
}
