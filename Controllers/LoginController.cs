using Barbearia.Models.ViewsModels;
using Barbearia.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Barbearia.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost] 
        public async Task<IActionResult> FazerLogin(LoginViewModel login)
        {
            // 1. Se o modelo estiver INVÁLIDO (faltou e-mail ou senha), volta para a tela de login
            if (!ModelState.IsValid)
            {
                return View("Index", login); // Especificamos "Index" para ele não procurar "FazerLogin"
            }

            // 2. Tenta realizar o login
            var sucesso = await _loginService.FazerLogin(login.Email, login.Senha);

            // 3. Se o login falhar no banco de dados
            if (!sucesso)
            {
                ModelState.AddModelError("", "Email ou senha inválidos");
                return View("Index", login); // Volta para a tela de login exibindo o erro
            }

            // 4. SE DEU TUDO CERTO: Transfere direto para a outra tela
            return RedirectToAction("RegistrarServicos", "Servicos");
        }
        public async Task<IActionResult> Logout()
        {
            await _loginService.Sair();
            return RedirectToAction("Login");
        }
    }
}
