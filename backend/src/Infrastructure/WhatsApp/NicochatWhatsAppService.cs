using Microsoft.Extensions.Configuration;
using SistemaTraction.Application.Common.Interfaces;

namespace SistemaTraction.Infrastructure.WhatsApp;

public class NicochatWhatsAppService(IConfiguration configuration) : IWhatsAppService
{
    private readonly string? _apiUrl = configuration["NICOCHAT_API_URL"];
    private readonly string? _apiKey = configuration["NICOCHAT_API_KEY"];

    public Task<WhatsAppResult> SendMessageAsync(string phoneNumber, string message, string recipientName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Task.FromResult(new WhatsAppResult(false, null, "Número do destinatário não configurado."));

        var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
        var waMeLink = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";

        if (string.IsNullOrWhiteSpace(_apiUrl) || string.IsNullOrWhiteSpace(_apiKey))
            return Task.FromResult(new WhatsAppResult(false, waMeLink, null));

        // Nicochat integration — Module 7
        return Task.FromResult(new WhatsAppResult(false, waMeLink, null));
    }
}
