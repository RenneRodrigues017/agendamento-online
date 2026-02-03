
using Microsoft.EntityFrameworkCore;
using Barbearia.Data;
using Barbearia.Interfaces;
using Barbearia.Models;

namespace Barbearia.Services
{
        public class GaleriaService : IGaleriaService
        {
            private readonly AppDbContext _context;

            public GaleriaService(AppDbContext context)
            {
                _context = context;
            }

            public async Task SalvarFotosAsync(List<IFormFile> fotos, Guid tenantId)
                {
                    var pastaGaleria = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/galeria");

                    // Garante que a pasta existe
                    if (!Directory.Exists(pastaGaleria))
                        Directory.CreateDirectory(pastaGaleria);

                    foreach (var foto in fotos)
                    {
                        if (foto == null || foto.Length == 0)
                            continue;

                        var nomeArquivo = $"{Guid.NewGuid()}_{Path.GetFileName(foto.FileName)}";
                        var caminhoFisico = Path.Combine(pastaGaleria, nomeArquivo);

                        // SALVA O ARQUIVO NO DISCO
                        using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        var entidade = new GaleriaFoto
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            ImagemPath = $"/images/galeria/{nomeArquivo}",
                            DataCadastro = DateTime.Now
                        };

                        _context.FotosGaleria.Add(entidade);
                    }

                    await _context.SaveChangesAsync();
                }


            public async Task<List<GaleriaFoto>> ObterFotosAsync(Guid tenantId)
            {
                return await _context.FotosGaleria
                    .Where(f => f.TenantId == tenantId)
                    .OrderByDescending(f => f.DataCadastro)
                    .ToListAsync();
            }

            public async Task ExcluirFotoAsync(Guid fotoId, Guid tenantId)
            {
                var foto = await _context.FotosGaleria
                    .FirstOrDefaultAsync(f => f.Id == fotoId && f.TenantId == tenantId);

                if (foto != null)
                {
                    _context.FotosGaleria.Remove(foto);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
