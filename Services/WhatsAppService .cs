
using Barbearia.Interfaces;

namespace Barbearia.Services
{
    public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;

    public WhatsAppService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task EnviarMensagemAsync(string telefone, string mensagem)
    {
        var payload = new
        {
            phone = telefone,
            message = mensagem
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.z-api.io/instances/SUA_INSTANCIA/token/SEU_TOKEN/send-text",
            payload
        );

        response.EnsureSuccessStatusCode();
    }
}

}