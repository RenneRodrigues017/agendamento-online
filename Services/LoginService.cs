using Barbearia.Data;
using Barbearia.Models;
using Microsoft.AspNetCore.Identity;

namespace Barbearia.Services
{
    public class LoginService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<bool> FazerLogin (string email, string senha)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null) 
                return false;

            var result = await _signInManager.PasswordSignInAsync(usuario, senha, isPersistent: true, lockoutOnFailure: false);

            return result.Succeeded;
        }

        public async Task Sair()
        {
             await _signInManager.SignOutAsync(); 
        }
    }
}
