using Barbearia.Data;
using Barbearia.Interfaces;
using Barbearia.Models;
using Barbearia.Models.ViewModels;
using Barbearia.Models.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AgendamentoController : Controller
{
    private readonly AppDbContext _context;
    private readonly ITenant _tenant;

    public AgendamentoController(AppDbContext context, ITenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IActionResult> Index()
    {
        if (_tenant.Id == Guid.Empty) return NotFound("Barbearia não encontrada.");

        // 1. Busca as configurações extras (se existirem)
        var config = await _context.ConfiguracaoBarbearias
            .FirstOrDefaultAsync(x => x.TenantId == _tenant.Id);

        // 2. Busca os dados do Tenant diretamente do Banco para garantir que o Endereço venha junto
        var dadosTenant = await _context.TenantModels
            .FirstOrDefaultAsync(t => t.Id == _tenant.Id);

        // Lógica de Prioridade: 
        // Primeiro tenta o endereço da Configuração, se for nulo, tenta o do Tenant.
        string enderecoFinal = config?.Endereco ?? dadosTenant?.Endereco ?? string.Empty;

        //Obter fotos da galeria
        var fotosGaleria = await _context.FotosGaleria
            .Where(g => g.TenantId == _tenant.Id)
            .ToListAsync();

        var model = new AgendamentoClienteViewModel
        {
            NomeBarbearia = config?.Nome ?? dadosTenant?.Nome ?? _tenant.Nome,
            EnderecoCompleto = enderecoFinal, // Agora ele deve encontrar!
            Galeria = fotosGaleria,

            Barbeiros = await ObterTodosProfissionais(_tenant.Id),
            Servicos = await _context.Servicos
                .Where(s => s.TenantId == _tenant.Id)
                .Select(s => new ServicoDTO
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Preco = s.Preco
                }).ToListAsync()
        };

        return View(model);
    }

    // Método auxiliar apenas para organizar a busca de profissionais
   private async Task<List<BarbeiroDTO>> ObterTodosProfissionais(Guid tenantId)
    {
        // 1. Busca os donos (Barbeiros) trazendo os bytes da foto do banco
        var donos = await _context.Barbeiros
            .Where(b => b.TenantId == tenantId)
            .Select(b => new BarbeiroDTO 
            { 
                Id = b.Id, 
                Nome = b.Nome, 
                ImagemPath = b.ImagemPath // Mapeia o caminho da foto
            })
            .ToListAsync();

        // 2. Busca os funcionários (Colaboradores) trazendo os bytes da foto do banco
        var colaboradores = await _context.Funcionarios
            .Where(f => f.TenantId == tenantId)
            .Select(f => new BarbeiroDTO 
            { 
                Id = f.Id, 
                Nome = f.Nome, 
                ImagemPath = f.ImagemPath // Mapeia o caminho da foto
            })
            .ToListAsync();

        // Une as duas listas e retorna
        return donos.Concat(colaboradores).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> ObterHorariosDisponiveis(Guid barbeiroId, DateTime data)
    {
        try
        {
            // Tenta buscar no banco
            var config = await _context.ConfiguracaoBarbearias
                .FirstOrDefaultAsync(x => x.TenantId == _tenant.Id);

            // Se não existir nada no banco ainda, definimos um padrão para não quebrar a tela
            var abertura = config?.HoraAbertura ?? new TimeSpan(8, 0, 0);
            var fechamento = config?.HoraFechamento ?? new TimeSpan(19, 0, 0);
            var intervalo = TimeSpan.FromMinutes(config?.IntervaloMinutos ?? 30);
            var inicioAlmoco = config?.AlmocoInicio ?? new TimeSpan(12, 0, 0);
            var fimAlmoco = config?.AlmocoFim ?? new TimeSpan(13, 0, 0);

            var ocupados = await _context.Agendamentos
                .Where(a => a.Profissional!.Id == barbeiroId && a.Horario.Date == data.Date)
                .Select(a => a.Horario.TimeOfDay)
                .ToListAsync();

            var horariosResult = new List<string>();
            for (var hora = abertura; hora < fechamento; hora = hora.Add(intervalo))
            {
                if (hora >= inicioAlmoco && hora < fimAlmoco) continue;

                if (!ocupados.Contains(hora))
                {
                    horariosResult.Add(hora.ToString(@"hh\:mm"));
                }
            }

            return Json(horariosResult);
        }
        catch (Exception ex)
        {
            // Se der qualquer erro, retorna uma lista vazia ou erro 400
            return BadRequest(new { mensagem = "Erro interno ao processar horários" });
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar([FromBody] FinalizarAgendamentoDTO model)
    {
        //Puxa o barbeiro do agendamento 
        var barbeirosExistente = await _context.Barbeiros.FindAsync(model.BarbeiroId);


        // Exemplo de validação no C#
        DateTime dataHoraAgendamento = DateTime.Parse($"{model.Data} {model.Hora}");
        if (dataHoraAgendamento < DateTime.Now)
        {
            return BadRequest("Não é possível agendar em um horário que já passou.");
        }
        // 1. Validação básica
        if (model == null || string.IsNullOrEmpty(model.Data) || string.IsNullOrEmpty(model.Hora))
            return BadRequest("Dados de agendamento incompletos.");

        try
        {
            // 2. Converter a Data (string "yyyy-MM-dd") e a Hora (string "HH:mm") em um único DateTime
            // model.Data vem do <input type="date">
            if (!DateTime.TryParse(model.Data, out DateTime dataBase))
            {
                return BadRequest("Formato de data inválido.");
            }

            var horaSplit = model.Hora.Split(':');
            int horas = int.Parse(horaSplit[0]);
            int minutos = int.Parse(horaSplit[1]);

            // Combinamos a data escolhida com o horário escolhido
            DateTime dataHoraAgendamento1 = dataBase.Date.AddHours(horas).AddMinutes(minutos);

            // 3. Verificação de segurança: Não permitir agendar no passado
            if (dataHoraAgendamento1 < DateTime.Now)
            {
                return BadRequest("Não é possível agendar em um horário que já passou.");
            }

            var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Celular == model.TelefoneCliente && c.TenantId == _tenant.Id);

            if (cliente == null)
            {
                // Se não existe, criamos o objeto mas NÃO precisamos dar SaveChanges agora, 
                // o EF salva tudo junto no final.
                cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = model.NomeCliente,
                    Celular = model.TelefoneCliente,
                    TenantId = _tenant.Id,
                    DataCadastro = DateTime.Now
                };
                _context.Clientes.Add(cliente);
            }

            // 4. Mapeamento para a Entidade de Banco
            var novoAgendamento = new Agendamento
            {
                Id = Guid.NewGuid(),
                Profissional = barbeirosExistente,
                ServicoId = model.ServicoId,
                Horario = dataHoraAgendamento1,
                Cliente = cliente,
                Status = StatusAgendamento.Pendente,
                TenantId = _tenant.Id
            };

            // 5. Salva no Banco
            _context.Agendamentos.Add(novoAgendamento);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Agendamento realizado com sucesso!" });

        }
        catch (Exception ex)
        {
            // Log do erro para o desenvolvedor
            Console.WriteLine($"Erro ao processar agendamento: {ex.Message}");
            return StatusCode(500, "Erro interno ao processar o agendamento.");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> ConfirmarPresenca(Guid id)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tenant.Id);

        if (agendamento == null)
            return NotFound();

        // 🔒 Regras
        if (agendamento.Status != StatusAgendamento.Pendente)
            return BadRequest("Agendamento não pode ser confirmado.");

        agendamento.Status = StatusAgendamento.Confirmado;

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Presença confirmada com sucesso!"
        });
    }

    [HttpPost]
    public async Task<IActionResult> CancelarAgendamentoCliente(Guid id)
    {
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tenant.Id);

        if (agendamento == null)
            return NotFound();

        // 🔒 Não pode cancelar concluído
        if (agendamento.Status == StatusAgendamento.Concluido)
            return BadRequest("Agendamento já foi concluído.");

        agendamento.Status = StatusAgendamento.Cancelado;

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Agendamento cancelado."
        });
    }

    [HttpPost]
    public IActionResult BuscarAgendamentosPorTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return BadRequest("Telefone inválido.");

        var cliente = _context.Clientes
            .FirstOrDefault(c => c.Celular == telefone);

        if (cliente == null)
            return NotFound("Nenhum cliente encontrado com esse telefone.");

        var agendamentos = _context.Agendamentos
            .Where(a => a.ClienteId == cliente.Id)
            .Include(a => a.Profissional)
            .OrderByDescending(a => a.Horario)
            .ToList();

        return PartialView("_AgendamentosClientePartial", agendamentos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarAgendamento([FromBody] AgendamentoAcaoViewModel model)
    {
        if (model.Id == Guid.Empty)
            return BadRequest(new { success = false, message = "ID inválido." });

        var agenda = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == model.Id && a.TenantId == _tenant.Id);

        if (agenda == null)
            return NotFound(new { success = false, message = "Agendamento não encontrado." });

        agenda.Status = StatusAgendamento.Cancelado;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Agendamento cancelado com sucesso!" });
    }
}