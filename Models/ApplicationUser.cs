using Microsoft.AspNetCore.Identity;

namespace Barbearia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Guid TenantId { get; set; }
        public TenantModels? Tenant { get; set; }
    }
}
