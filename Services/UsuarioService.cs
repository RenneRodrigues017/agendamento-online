using Barbearia.Data;
using Barbearia.Interfaces;
using Barbearia.Models;
using Barbearia.Models.ViewsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Necessário para transações

public class UsuarioService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public UsuarioService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<(bool Sucesso, string Erro)> CadastrarBarbeiroDonoAsync(BarbeiroRegisterViewModel model)
    {
        // Iniciamos uma transação para garantir que, se algo falhar, 
        // não criemos um Tenant sem usuário ou um Usuário sem Barbeiro.
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. CRIAR O NOVO TENANT (A BARBEARIA)
            var novoTenant = new TenantModels 
            {
                Id = Guid.NewGuid(),
                Nome = model.NomeBarbearia,
                Subdominio = model.NomeBarbearia.Replace(" ", "").ToLower(),
                Endereco = model.Endereco
            };

            _dbContext.TenantModels.Add(novoTenant);
            await _dbContext.SaveChangesAsync();

            // 2. CRIAÇÃO DO APPLICATIONUSER (Identity)
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                TenantId = novoTenant.Id, // Vincula ao tenant recém-criado
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Senha);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();  //Desfaz tudo, se aglo der errado nas transações no banco

                var erros = string.Join(", ", result.Errors.Select(e => e.Description)); //Divide os erros de forma organizada para o usuario 

                return (false, erros);
            }

            // 3. CRIAÇÃO DO MODEL DE NEGÓCIO (Barbeiro)
            var barbeiro = new Barbeiro
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Nome = model.Nome,
                TenantId = novoTenant.Id, // Vincula ao mesmo tenant
            };

            _dbContext.Barbeiros.Add(barbeiro);
            await _dbContext.SaveChangesAsync();

            // 4. FINALIZAÇÃO
            await transaction.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Erro crítico no cadastro: " + ex.Message);
        }
    }
}