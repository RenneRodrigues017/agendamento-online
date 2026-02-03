using Barbearia.Models;
using Barbearia.Models.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// Crie o Controller, injetando o seu Service no construtor
public class UsuarioController : Controller
{
    private readonly UsuarioService _usuarioService;

    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    // GET: Exibe o formulário
    public IActionResult CadastrarBarbeiro()
    {
        return View();
    }

    // POST: Recebe o formulário e chama o Service
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarBarbeiro(BarbeiroRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (sucesso, erro) = await _usuarioService.CadastrarBarbeiroDonoAsync(model);

        if (sucesso)
        {
            TempData["SuccessMessage"] = "Barbeiro cadastrado com sucesso!";
            return RedirectToAction("Index", "Login");
        }
        else
        {
            ModelState.AddModelError(string.Empty, erro);
            return View(model);
        }
    }
}