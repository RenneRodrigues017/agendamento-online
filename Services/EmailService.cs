using Barbearia.Interfaces;
using Barbearia.Models.ViewsModels;
using MailKit.Net.Smtp;
using MimeKit;

namespace SeuProjeto.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> EnviarEmailContatoAsync(ContatoViewModel dados)
        {
            var email = new MimeMessage();
            // Usamos o Username do config tanto para o From quanto para a Autenticação
            var emailRemetente = _config["EmailSettings:Username"];
            var senhaApp = _config["EmailSettings:Password"];
            var servidorSmtp = _config["EmailSettings:SmtpServer"];
            var portaSmtp = int.Parse(_config["EmailSettings:Port"] ?? "587");

            email.From.Add(new MailboxAddress("BarberFlow Site", emailRemetente));
            email.To.Add(new MailboxAddress("Administrador", "rennefrancisco9@gmail.com"));
            email.Subject = "Novo Lead: Barbeiro Interessado";

            email.Body = new TextPart("html")
            {
                Text = $@"
            <h1>Novo contato pelo site</h1>
            <p><strong>Nome:</strong> {dados.Nome}</p>
            <p><strong>E-mail:</strong> {dados.Email}</p>
            <hr>
            <p><strong>Mensagem:</strong></p>
            <p>{dados.Mensagem}</p>"
            };

            using var client = new SmtpClient();
            try
            {
                // Conecta usando as variáveis do appsettings
                await client.ConnectAsync(servidorSmtp, portaSmtp, MailKit.Security.SecureSocketOptions.StartTls);

                // Autentica com a sua Senha de App de 16 dígitos
                await client.AuthenticateAsync(emailRemetente, senhaApp);

                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // Log do erro para você saber o que aconteceu se falhar
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                return false;
            }
        }
    }
}