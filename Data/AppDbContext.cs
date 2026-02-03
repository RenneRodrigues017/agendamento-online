using Barbearia.Interfaces;
using Barbearia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Barbearia.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenant _tenant;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenant tenant)
            : base(options)
        {
            _tenant = tenant;
        }

        // Tabela de barbeiros no banco de dados
        public DbSet<Barbeiro> Barbeiros { get; set; }

        // Tabela de agendamentos no banco de dados
        public DbSet<Agendamento> Agendamentos { get; set; }

        // Tabela de tenants no banco de dados
        public DbSet<TenantModels> TenantModels { get; set; }
        
        //Tabela funcionarios da barbearia 
        public DbSet<Funcionario> Funcionarios { get; set; }

        //Tabela de serviços de cada barbearia 
        public DbSet<Servico> Servicos { get; set; }

        //Tabela de configurações de cada barbearia
        public DbSet<ConfiguracaoBarbearia> ConfiguracaoBarbearias { get; set; }

        //Tabela de clientes da barbearia
        public DbSet<Cliente> Clientes { get; set; }

        //Tabela de fotos da galeria
        public DbSet<GaleriaFoto> FotosGaleria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento Barbeiro -> ApplicationUser (Identity)
            modelBuilder.Entity<Barbeiro>()
                .HasOne(b => b.User)
                .WithOne()
                .HasForeignKey<Barbeiro>(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Servico>()
                .Property(s => s.Preco)
                .HasPrecision(18, 2);

            // Filtro global por Tenant
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenant).IsAssignableFrom(entityType.ClrType))
                {
                    var clrType = entityType.ClrType;

                    var parameter = Expression.Parameter(clrType, "e");

                    var propertyMethod = typeof(EF).GetMethod("Property")
                        ?.MakeGenericMethod(typeof(Guid));

                    var propertyExpression = Expression.Call(
                        propertyMethod!,
                        parameter,
                        Expression.Constant("TenantId")
                    );

                    var equality = Expression.Equal(
                        propertyExpression,
                        Expression.Constant(_tenant.Id)
                    );

                    var lambda = Expression.Lambda(equality, parameter);

                    modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                }
            }
        }
    }
}
