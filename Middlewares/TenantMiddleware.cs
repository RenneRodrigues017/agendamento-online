using Barbearia.Data;
using Barbearia.Interfaces;
using Barbearia.Services;
using Microsoft.EntityFrameworkCore;

namespace Barbearia.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext _appDbContext, ITenant tenant)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/images") ||
                path.StartsWith("/login") ||
                path.StartsWith("/account") ||
                path.StartsWith("/usuario/cadastrarbarbeiro")
            )
            {
                await _next(context);
                return;
            }


            Guid? tenantIdEncontrado = null;
            string nomeTenantEncontrado = null;

            // 1. TENTATIVA PELA URL (Subdomínio)
            var host = context.Request.Host.Host;
            string tenantIdentifier = "";

            if (host.Contains(".")) {
                var parts = host.Split('.');
                tenantIdentifier = parts[0].ToLower();
            }
            // Adicionado AsNoTracking() e TagWith para facilitar debug no log do SQL
            var tenantPelaUrl = await _appDbContext.TenantModels
                .AsNoTracking()
                .TagWith("BuscaTenantMiddleware")
                .FirstOrDefaultAsync(x => x.Subdominio == tenantIdentifier);

            if (tenantPelaUrl != null)
            {
                tenantIdEncontrado = tenantPelaUrl.Id;
                nomeTenantEncontrado = tenantPelaUrl.Nome;
            }

            // 2. TENTATIVA PELO USUÁRIO LOGADO
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var claimTenantId = context.User.FindFirst("TenantId")?.Value;
                if (Guid.TryParse(claimTenantId, out Guid idDoUsuario))
                {
                    tenantIdEncontrado = idDoUsuario;

                    if (string.IsNullOrEmpty(nomeTenantEncontrado))
                    {
                        // FindAsync não aceita AsNoTracking, então usamos FirstOrDefault
                        var t = await _appDbContext.TenantModels
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == idDoUsuario);

                        nomeTenantEncontrado = t?.Nome;
                    }
                }
            }

            // 3. PREENCHE O CONTEXTO
            if (tenantIdEncontrado.HasValue && tenant is TenantService tenantContext)
            {
                tenantContext.Id = tenantIdEncontrado.Value;
                tenantContext.Nome = nomeTenantEncontrado;
            }

            await _next(context);
        }
    }
}