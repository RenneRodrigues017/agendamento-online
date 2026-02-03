using Barbearia.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Barbearia.Models
{
    public class Barbeiro  
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatorio.")]
        [StringLength(50, ErrorMessage = "Nome deve ter menos que 50 caracteres.")]
        public string? Nome { get; set; }
       
        // FK que armazena o Id do usuário do Identity relacionado a este barbeiro
        public string? UserId { get; set; } 

        // Navegação para o usuário do Identity relacionado a este barbeiro
        public ApplicationUser User { get; set; } = null!;

        public Guid TenantId { get; set; }

        public string? ImagemPath { get; set; }

        // Referência à empresa (Tenant) proprietária deste registro
        public TenantModels? Tenant { get; set; }
    }
}
