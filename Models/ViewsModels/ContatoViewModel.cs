using System.ComponentModel.DataAnnotations;

namespace Barbearia.Models.ViewsModels;

public class ContatoViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome é muito longo.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A mensagem não pode estar vazia.")]
    [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo 1000 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;
}