using Barbearia.Interfaces;
using Barbearia.Models;
using Barbearia.Models.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Barbearia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        public HomeController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarContato(ContatoViewModel model)
        {
            // Verificação para debug (pode remover depois)
            if (model == null) return Json(new { success = false, message = "Dados vazios." });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dados inválidos no formulário." });
            }

            try
            {
                var sucesso = await _emailService.EnviarEmailContatoAsync(model);
                if (sucesso)
                {
                    return Json(new { success = true, message = "Mensagem enviada com sucesso!" });
                }
                return Json(new { success = false, message = "O serviço de e-mail falhou ao processar." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno: " + ex.Message });
            }
        }
    }
}
