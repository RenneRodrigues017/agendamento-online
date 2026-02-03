using Barbearia.Models;

public class FinalizarAgendamentoDTO
{
    public Guid BarbeiroId { get; set; }
    public Guid ServicoId { get; set; }
    public string? Hora { get; set; }
    public string? NomeCliente { get; set; }
    public string? TelefoneCliente { get; set; }
    public string? Data { get; set; }
    public string? NomeProfissional { get; set; }

    public Guid TenantId { get; set; }
}