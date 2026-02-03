using Barbearia.Interfaces;
using Barbearia.Models;
using Barbearia.Models.ViewsModels;
using Barbearia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Barbearia.Controllers
{
    public class ServicosController : Controller
    {
        private readonly ServicosService _configService;
        private readonly ITenant _tenant;
        private readonly IGaleriaService _galeriaService;

        public ServicosController(ServicosService configService, ITenant tenant, IGaleriaService galeriaService)
        {
            _configService = configService;
            _tenant = tenant;
            _galeriaService = galeriaService;
        }
        [HttpGet]
        public async Task<IActionResult> RegistrarServicos()
        {
            var model = await _configService.ObterDadosConfiguracao(_tenant.Id);
            model.FotosGaleria = await _galeriaService.ObterFotosAsync(_tenant.Id);

            // Adicione esta linha para gerar a grade ao carregar a página
            ViewBag.GradeGerada = _configService.GerarGradeHorarios(model);

            return View(model);
        }

        //CADASTRA O FUNCIONARIO DE CADA BARBEARIA 
        [HttpPost]
        public async Task<IActionResult> CadastrarFuncionario(ConfiguracaoViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Nome))
            {
                await _configService.SalvarFuncionario(model.Nome, _tenant.Id);
                return RedirectToAction("RegistrarServicos");
            }
            return View("RegistrarServicos", model);
        }

        //CONFIGURA A AGENDA DE CADA BARBEARIA 
        [HttpPost]
        public async Task<IActionResult> ConfigurarAgenda(ConfiguracaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Salva as configurações permanentemente no banco vinculado ao Tenant
                await _configService.SalvarConfiguracaoAgenda(model, _tenant.Id);

                // 2. Opcional: Gera a grade para visualização imediata (como você já fazia)
                var grade = _configService.GerarGradeHorarios(model);
                ViewBag.GradeGerada = grade;

                // Adicione uma mensagem de sucesso para o usuário
                TempData["MensagemSucesso"] = "Configurações da agenda salvas com sucesso!";

                return RedirectToAction("RegistrarServicos");
            }

            return View("RegistrarServicos", model);
        }

        //CADASTRA OS SERVIÇOS DE CADA BARBEARIA 
        [HttpPost]
        public async Task<IActionResult> CadastrarServico(string nomeServico, decimal preco)
        {
            if (!string.IsNullOrEmpty(nomeServico))
            {
                // Supomos que o SalvarServico retorna o ID gerado ou o objeto completo
                var servicoId = await _configService.SalvarServico(nomeServico, preco, _tenant.Id);

                // Retornamos um JSON com os dados para o JavaScript usar
                return Json(new
                {
                    success = true,
                    id = servicoId, // Certifique-se que o SalvarServico retorne o ID
                    nome = nomeServico,
                    preco = preco.ToString("F2")
                });
            }

            return Json(new { success = false, message = "Nome inválido" });
        }
        [HttpPost]
        public async Task<IActionResult> AtualizarIdentidade(string nomeBarbearia, string endereco)
        {
            if (!string.IsNullOrEmpty(nomeBarbearia) || !string.IsNullOrEmpty(endereco))
            {
                await _configService.AtualizarDadosIdentidade(nomeBarbearia, endereco, _tenant.Id);
                TempData["MensagemSucesso"] = "Dados da barbearia atualizados!";
            }

            return RedirectToAction("RegistrarServicos");
        }
        [HttpPost]
        public async Task<IActionResult> ExcluirServico(Guid id)
        {
            try
            {
                await _configService.ExcluirServico(id, _tenant.Id);
                // Retorna sucesso sem pedir para a página recarregar
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Erro ao excluir serviço");
            }
        }
        // Adicione este método ao seu ServicosController
        [HttpPost]
        public async Task<IActionResult> ExcluirFuncionario(Guid id)
        {
            await _configService.ExcluirFuncionario(id, _tenant.Id);
            TempData["MensagemSucesso"] = "Profissional removido com sucesso!";
            return RedirectToAction("RegistrarServicos");
        }
        [HttpPost]
        public async Task<IActionResult> AtualizarPreco(Guid id, decimal preco)
        {
            try
            {
                await _configService.AtualizarPrecoServico(id, preco, _tenant.Id);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAgendamento([FromBody] AgendamentoAcaoViewModel model)
        {
            if (model.Id == Guid.Empty)
                return BadRequest(new { success = false, message = "ID inválido." });

            await _configService.MudarStatusAgendamento(
                model.Id,
                StatusAgendamento.Concluido,
                _tenant.Id
            );

            return Json(new { success = true, message = "Atendimento concluído com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAgendamento([FromBody] AgendamentoAcaoViewModel model)
        {
            if (model.Id == Guid.Empty)
                return BadRequest(new { success = false, message = "ID inválido." });

            await _configService.MudarStatusAgendamento(
                model.Id,
                StatusAgendamento.Cancelado,
                _tenant.Id
            );

            return Json(new { success = true, message = "Agendamento cancelado com sucesso!" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarFotoFuncionario(Guid id, IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest("Imagem inválida");

            // 1️⃣ Salva o arquivo físico
            var imagemPath = await _configService.SalvarImagemFuncionario(foto);

            // 2️⃣ Atualiza somente o caminho no banco
            await _configService.AtualizarFotoFuncionario(
                id,
                _tenant.Id,
                imagemPath
            );

            // 3️⃣ Retorna a URL para atualizar a tela
            return Json(new
            {
                success = true,
                fotoUrl = imagemPath
            });
        }


        [HttpPost]
        public async Task<IActionResult> EditarNomeFuncionario(Guid id, string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return BadRequest(new { message = "O nome não pode estar vazio." });

                await _configService.AtualizarNomeFuncionario(id, nome, _tenant.Id);

                return Json(new { success = true, message = "Nome atualizado com sucesso!" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Erro ao atualizar o nome." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarServico([FromBody] EditarServicoDTO dto)
        {
            try
            {
                if (dto.Id == Guid.Empty)
                    return BadRequest(new { message = "ID inválido." });

                if (string.IsNullOrWhiteSpace(dto.Nome) && !dto.Preco.HasValue)
                    return BadRequest(new { message = "Nenhum dado informado para atualização." });

                await _configService.AtualizarDadosServico(
                    dto.Id,
                    dto.Nome,
                    dto.Preco,
                    _tenant.Id
                );

                return Json(new { success = true, message = "Serviço atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao atualizar o serviço: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadGaleria(List<IFormFile> Fotos)
        {
            if (Fotos == null || !Fotos.Any())
                return Json(new { success = false, message = "Nenhuma foto selecionada." });

            await _galeriaService.SalvarFotosAsync(Fotos, _tenant.Id);

            return Json(new { success = true, message = "Fotos adicionadas à galeria!" });
        }


        [HttpPost]
        public async Task<IActionResult> ExcluirFotoGaleria(Guid id)
        {
            await _galeriaService.ExcluirFotoAsync(id, _tenant.Id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ConcluirAgendamento(Guid id)
        {
            try
            {
                await _configService.ConcluirAgendamentoAsync(id, _tenant.Id);

                return Json(new
                {
                    success = true,
                    message = "Atendimento concluído com sucesso!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }   
}
