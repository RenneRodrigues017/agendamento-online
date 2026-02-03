namespace Barbearia.Models
{
    public class Funcionario
    {
        public  Guid Id { get; set; }
        public string? Nome { get; set; }
        public Guid TenantId { get; set; }
        public string? ImagemPath { get; set; }
        
    }
}
