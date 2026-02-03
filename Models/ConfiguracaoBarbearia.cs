namespace Barbearia.Models
{
    public class ConfiguracaoBarbearia
    {
        public Guid Id { get; set; }
        public TimeSpan HoraAbertura { get; set; }
        public TimeSpan HoraFechamento { get; set; }
        public TimeSpan AlmocoInicio { get; set; }
        public TimeSpan AlmocoFim { get; set; }
        public int IntervaloMinutos { get; set; }
        public Guid TenantId { get; set; } // Vincula à barbearia
        public string? Nome { get; set; }
        public string? Endereco { get; set; }
    }
}
