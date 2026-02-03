namespace Barbearia.Models.ViewModels
{
    public class AgendamentoClienteViewModel
    {
        public string? NomeBarbearia { get; set; }
        public List<BarbeiroDTO>? Barbeiros { get; set; }
        public List<ServicoDTO>? Servicos { get; set; }
        public string? EnderecoCompleto { get; set; }
        public StatusAgendamento Status { get; set; }

        public List<Agendamento> AgendamentosCliente { get; set; } = new();

        public List<GaleriaFoto>? Galeria { get; set; }
    }

    public class BarbeiroDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? ImagemPath { get; set; }
    }

    public class ServicoDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public decimal Preco { get; set; }
    }
}