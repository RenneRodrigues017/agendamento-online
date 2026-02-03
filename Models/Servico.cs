using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barbearia.Models
{
    public class Servico
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        // Relacionamento com o Tenant (Multi-tenant)
        public Guid TenantId { get; set; }
        public TenantModels? Tenant { get; set; }
    }
}