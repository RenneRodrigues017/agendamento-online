using Barbearia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Barbearia.Identity
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        // Este método é chamado pelo Identity no momento do Login
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Adiciona o TenantId como um "crachá" (Claim)
            // Assim, o Middleware e os Controllers podem ler sem ir ao banco
            identity.AddClaim(new Claim("TenantId", user.TenantId.ToString()));

            return identity;
        }
    }
}