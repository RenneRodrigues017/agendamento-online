namespace Barbearia.Models
{
    public class Agendamento
    {
        
        public Guid Id { get; set; }
        public DateTime Horario { get; set; } // Data e hora do agendamento
        public Cliente? Cliente { get; set; }
        
        public Guid? ClienteId { get; set; }

        // O profissional que vai atender (Pode ser o ID da tabela Barbeiros ou Funcionarios)
        public Barbeiro? Profissional { get; set; }

        public Guid? ProfissionalId { get; set; }

        // O serviço que será realizado (Corte, Barba, etc)
        public Guid ServicoId { get; set; }

        public Guid TenantId { get; set; }
        public TenantModels? Tenant { get; set; }

        public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;

        public bool Notificacao24hEnviada { get; set; }
        public DateTime? DataNotificacao24h { get; set; }

    }
    public enum StatusAgendamento
    {
        Pendente = 1,
        Confirmado = 2,
        Concluido = 3,
        Cancelado = 4,
    }

}
