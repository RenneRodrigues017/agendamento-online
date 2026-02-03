using Barbearia.Data;
using Barbearia.Identity; // Adicione o namespace onde está a CustomClaimsPrincipalFactory
using Barbearia.Interfaces;
using Barbearia.Middlewares;
using Barbearia.Models;
using Barbearia.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeuProjeto.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------
// 1) DbContext
// --------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddDbContext<AppDbContextSemTenant>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// --------------------------------------
// 2) Multi-tenant - Services
// --------------------------------------
builder.Services.AddScoped<ITenant, TenantService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ServicosService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IGaleriaService, GaleriaService>();




// --------------------------------------
// 3) Identity
// --------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    // --- AJUSTE AQUI: Registra a fábrica que coloca o TenantId no "crachá" do usuário ---
    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();

// --------------------------------------
// 4) MVC
// --------------------------------------
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --------------------------------------
// AJUSTE DE ORDEM NO PIPELINE
// --------------------------------------

// 1. Primeiro o Identity "abre o envelope" (Cookie) e identifica quem é o usuário
app.UseAuthentication();

// 2. AGORA o Middleware roda. Como o usuário já foi identificado, 
// o Middleware consegue ler o TenantId dos Claims (crachá) dele.
app.UseMiddleware<TenantMiddleware>();

// 3. Depois verificamos o que ele pode acessar
app.UseAuthorization();

// Rotas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();