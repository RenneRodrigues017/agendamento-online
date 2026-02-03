using System;
using Barbearia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Barbearia.Data
{
    public class AppDbContextSemTenant : IdentityDbContext<ApplicationUser>
    {
        public AppDbContextSemTenant(DbContextOptions<AppDbContextSemTenant> options)
            : base(options)
        {
        }

        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<TenantModels> TenantModels { get; set; }
    }
}