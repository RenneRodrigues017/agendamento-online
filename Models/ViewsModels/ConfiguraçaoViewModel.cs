using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Barbearia.Models.ViewsModels
{
    public class ConfiguracaoViewModel
    {
        // Funcionário
        public string? Nome { get; set; }

        // Serviço
        public string? NomeServico { get; set; }
        public decimal Preco { get; set; }

        // Agenda
        public int IntervaloMinutos { get; set; }
        public TimeSpan HoraAbertura { get; set; }
        public TimeSpan HoraFechamento { get; set; }
        public TimeSpan HoraAlmocoInicio { get; set; }
        public TimeSpan HoraAlmocoFim { get; set; }
        public List<Servico> Servicos { get; set; } = new List<Servico>();
        public List<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();

        public string? Endereco { get; set; }// No arquivo ConfiguracaoViewModel.cs
        public List<AgendamentoExibicaoViewModel> Agendamentos { get; set; } = new();

        public List<GaleriaFoto> FotosGaleria { get; set; } = new();

        public List<ProfissionalAgendaVM>? Profissionais { get; set; }

        // Classe de apoio para a lista
        public class AgendamentoExibicaoViewModel
        {
            public Guid Id { get; set; }
            public Guid? ProfissionalId { get; set; }
            public string? ClienteNome { get; set; }
            public string? ServicoNome { get; set; }
            public string? FuncionarioNome { get; set; }
            public string? ClienteTelefone { get; set; }
            public DateTime Horario { get; set; }
            public StatusAgendamento Status { get; set; }
            public decimal Valor { get; set; }
        }

    }
}
