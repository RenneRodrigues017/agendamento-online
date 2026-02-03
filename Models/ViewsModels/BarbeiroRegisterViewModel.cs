using System.ComponentModel.DataAnnotations;

namespace Barbearia.Models.ViewsModels
{
    // Usada para receber os dados do formulário de cadastro de um novo barbeiro
    public class BarbeiroRegisterViewModel
    {
        //Nome do dono da barbearia 
        [Required]
        [StringLength(50)]
        public string? Nome { get; set; }

        //Email do dono
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        public string Email { get; set; } = string.Empty;

        //Senha do email
        [Required(ErrorMessage = "A Senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;


        //Nome da barbearia
        [Required(ErrorMessage = "O nome da barbearia é obrigatório")]
        [Display(Name = "Nome da Barbearia")]
        public string? NomeBarbearia { get; set; }

        public string? Endereco { get; set; }
    }
}