using Barbearia.Data;
using Barbearia.Interfaces;
using Barbearia.Models;
using Barbearia.Models.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Barbearia.Models.ViewsModels.ConfiguracaoViewModel;

namespace Barbearia.Services
{
    public class ServicosService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ServicosService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<TimeSpan> GerarGradeHorarios(ConfiguracaoViewModel config)
        {
            var horarios = new List<TimeSpan>();
            var horarioAtual = config.HoraAbertura;

            while (horarioAtual < config.HoraFechamento)
            {
                // Verifica se o horário atual está fora do intervalo de almoço
                if (horarioAtual < config.HoraAlmocoInicio || horarioAtual >= config.HoraAlmocoFim)
                {
                    horarios.Add(horarioAtual);
                }

                // Soma o intervalo (ex: 30 minutos)
                horarioAtual = horarioAtual.Add(TimeSpan.FromMinutes(config.IntervaloMinutos));
            }

            return horarios;
        }

        //SALVAR O FUNCIONARIO DE CADA BARBEARIA
        public async Task SalvarFuncionario(string nome, Guid tenantId)
        {
            var funcionario = new Funcionario
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                TenantId = tenantId
            };
            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();
        }

        //SALVAR O SERVIÇO DE CADA BARBEARIA 
        public async Task<Servico> SalvarServico(string nome, decimal preco, Guid tenantId)
        {
            var novoServico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Preco = preco,
                TenantId = tenantId
            };

            _context.Servicos.Add(novoServico);
            await _context.SaveChangesAsync();

            return novoServico;
        }

        //SALVAR AS CONFIGURAÇÕES DE CADA BARBEARIA 
        public async Task SalvarConfiguracaoAgenda(ConfiguracaoViewModel model, Guid tenantId)
        {
            var configExistente = await _context.ConfiguracaoBarbearias
                .FirstOrDefaultAsync(x => x.TenantId == tenantId);

            if (configExistente == null)
            {
                configExistente = new ConfiguracaoBarbearia { Id = Guid.NewGuid(), TenantId = tenantId };
                _context.ConfiguracaoBarbearias.Add(configExistente);
            }

            configExistente.HoraAbertura = model.HoraAbertura;
            configExistente.HoraFechamento = model.HoraFechamento;
            configExistente.AlmocoInicio = model.HoraAlmocoInicio;
            configExistente.AlmocoFim = model.HoraAlmocoFim;
            configExistente.IntervaloMinutos = model.IntervaloMinutos;

            await _context.SaveChangesAsync();
        }

        // ATUALIZAR NOME E ENDEREÇO DA BARBEARIA
        public async Task AtualizarDadosIdentidade(string nomeBarbearia, string endereco, Guid tenantId)
        {
            var tenant = await _context.TenantModels.FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant != null)
            {
                tenant.Nome = nomeBarbearia;
                tenant.Endereco = endereco;

                _context.TenantModels.Update(tenant);
                await _context.SaveChangesAsync();
            }
        }

        // Método para obter os dados de configuração de uma barbearia
        public async Task<ConfiguracaoViewModel> ObterDadosConfiguracao(Guid tenantId)
        {
            var model = new ConfiguracaoViewModel();

            // 1. Busca serviços e funcionários
            model.Servicos = await _context.Servicos
                .Where(s => s.TenantId == tenantId)
                .ToListAsync();

            model.Funcionarios = await _context.Funcionarios
                .Where(f => f.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();

            // --- NOVIDADE: Busca os donos (Barbeiros) ---
            var donos = await _context.Barbeiros
                .Where(b => b.TenantId == tenantId)
                .ToListAsync();

            if (donos != null)
            {
                foreach (var d in donos)
                {
                    // Verificamos se ele já não está na lista (evitar duplicidade)
                    if (!model.Funcionarios.Any(f => f.Id == d.Id))
                    {
                        model.Funcionarios.Insert(0, new Funcionario
                        {
                            Id = d.Id,
                            Nome = d.Nome,           // Nome atualizado do banco
                            ImagemPath = d.ImagemPath,     // FOTO atualizada do banco
                            TenantId = tenantId
                        });
                    }
                }
            }

            // 2. Busca agendamentos
            var agendamentosDb = await _context.Agendamentos
                .Where(a => a.TenantId == tenantId)
                .AsNoTracking()
                .Select(a => new
                {
                    a.Id,
                    a.Horario,
                    a.Status,
                    a.ServicoId,
                    ClienteNome = a.Cliente.Nome,
                    ProfissionalId = a.ProfissionalId
                })
                .ToListAsync();


            var servicosDict = model.Servicos.ToDictionary(s => s.Id);
            var funcionariosDict = model.Funcionarios.ToDictionary(f => f.Id);
            var donosDict = donos.ToDictionary(d => d.Id);

            // 3. Transforma cruzando com as DUAS listas (Funcionários e Donos)
            model.Agendamentos = agendamentosDb
            .Select(a =>
            {
                servicosDict.TryGetValue(a.ServicoId, out var servico);

                string nomeProfissional = null;

                if (a.ProfissionalId.HasValue)
                {
                    if (!funcionariosDict.TryGetValue(a.ProfissionalId.Value, out var func))
                    {
                        donosDict.TryGetValue(a.ProfissionalId.Value, out var dono);
                        nomeProfissional = dono?.Nome;
                    }
                    else
                    {
                        nomeProfissional = func.Nome;
                    }
                }

                return new ConfiguracaoViewModel.AgendamentoExibicaoViewModel
                {
                    Id = a.Id,
                    ProfissionalId = a.ProfissionalId,
                    ClienteNome = a.ClienteNome ?? "Não informado",
                    Horario = a.Horario,
                    ServicoNome = servico?.Nome ?? "Serviço removido",
                    FuncionarioNome = nomeProfissional ?? "Profissional removido",
                    Status = a.Status,
                    Valor = servico?.Preco ?? 0
                };
            })
            .OrderBy(a => a.Horario)
            .ToList();


            // 4. Busca configurações (o resto do seu código continua igual...)
            var config = await _context.ConfiguracaoBarbearias
                .FirstOrDefaultAsync(x => x.TenantId == tenantId);
            var tenantInfo = await _context.TenantModels // Verifique se o nome no seu DbContext é "Tenants" ou "Barbearias"
            .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenantInfo != null)
            {
                model.Nome = tenantInfo.Nome; // Nome da Barbearia que vem do Tenant
                model.Endereco = tenantInfo.Endereco; // Endereço que vem do Tenant
            }

            if (config != null)
            {
                model.HoraAbertura = config.HoraAbertura;
                model.HoraFechamento = config.HoraFechamento;
                model.HoraAlmocoInicio = config.AlmocoInicio;
                model.HoraAlmocoFim = config.AlmocoFim;
                model.IntervaloMinutos = config.IntervaloMinutos;
            }

            return model;
        }

        // Metodo para excluir um serviço
        public async Task ExcluirServico(Guid id, Guid tenantId)
        {
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (servico != null)
            {
                _context.Servicos.Remove(servico);
                await _context.SaveChangesAsync();
            }
        }

        // Adicione este método ao final da classe ServicosService
        public async Task ExcluirFuncionario(Guid id, Guid tenantId)
        {
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (funcionario != null)
            {
                _context.Funcionarios.Remove(funcionario);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AtualizarPrecoServico(Guid id, decimal novoPreco, Guid tenantId)
        {
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (servico != null)
            {
                servico.Preco = novoPreco;
                await _context.SaveChangesAsync();
            }
        }
        public async Task ConfirmarAtendimento(Guid agendamentoId, Guid tenantId)
        {
            var agendamento = await _context.Agendamentos
                .FirstOrDefaultAsync(a => a.Id == agendamentoId && a.TenantId == tenantId);

            if (agendamento != null)
            {
                agendamento.Status = StatusAgendamento.Concluido;
                await _context.SaveChangesAsync();
            }
        }
        public async Task MudarStatusAgendamento(Guid agendamentoId, StatusAgendamento novoStatus, Guid tenantId)
        {
            var agendamento = await _context.Agendamentos
                .FirstOrDefaultAsync(a => a.Id == agendamentoId && a.TenantId == tenantId);

            if (agendamento != null)
            {
                agendamento.Status = novoStatus;
                await _context.SaveChangesAsync();
            }
        }
        public async Task AtualizarFotoFuncionario(Guid id, Guid tenantId, string imagemPath)
        {
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (funcionario != null)
            {
                funcionario.ImagemPath = imagemPath;
                await _context.SaveChangesAsync();
                return;
            }

            var barbeiro = await _context.Barbeiros
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (barbeiro != null)
            {
                barbeiro.ImagemPath = imagemPath;
                await _context.SaveChangesAsync();
                return;
            }

            throw new Exception("Funcionário ou barbeiro não encontrado");
        }

        public async Task AtualizarNomeFuncionario(Guid id, string novoNome, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(novoNome)) return;

            // 1. Tenta na tabela de Funcionários
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (funcionario != null)
            {
                funcionario.Nome = novoNome;
                _context.Funcionarios.Update(funcionario);
            }
            else
            {
                // 2. Tenta na tabela de Barbeiros (Donos)
                var barbeiro = await _context.Barbeiros
                    .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

                if (barbeiro != null)
                {
                    barbeiro.Nome = novoNome;
                    _context.Barbeiros.Update(barbeiro);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task AtualizarDadosServico(Guid id, string? nome, decimal? preco, Guid tenantId)
        {
            // Localiza o serviço garantindo que pertence à barbearia logada (Tenant)
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (servico == null)
                throw new Exception("Serviço não encontrado.");

            // Atualiza apenas o que foi preenchido
            if (!string.IsNullOrWhiteSpace(nome))
                servico.Nome = nome;

            if (preco.HasValue)
                servico.Preco = preco.Value;

            _context.Servicos.Update(servico);
            await _context.SaveChangesAsync();
        }

        public async Task ConcluirAgendamentoAsync(Guid agendamentoId, Guid tenantId)
        {
            var agendamento = await _context.Agendamentos
                .FirstOrDefaultAsync(a =>
                    a.Id == agendamentoId &&
                    a.TenantId == tenantId);

            if (agendamento == null)
                throw new Exception("Agendamento não encontrado.");

            // 🔒 Regras de negócio
            if (agendamento.Status == StatusAgendamento.Cancelado)
                throw new Exception("Agendamento cancelado não pode ser concluído.");

            if (agendamento.Status == StatusAgendamento.Concluido)
                throw new Exception("Agendamento já foi concluído.");

            agendamento.Status = StatusAgendamento.Concluido;

            await _context.SaveChangesAsync();
        }

        public async Task<string> SalvarImagemFuncionario(IFormFile imagem)
        {
            var pasta = Path.Combine(_environment.WebRootPath, "images", "funcionarios");

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(imagem.FileName)}";
            var caminhoFisico = Path.Combine(pasta, nomeArquivo);

            using (var stream = new FileStream(caminhoFisico, FileMode.Create))
            {
                await imagem.CopyToAsync(stream);
            }

            return $"/images/funcionarios/{nomeArquivo}";
        }

    
    }
}