namespace SistemaTraction.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task<WhatsAppResult> SendMessageAsync(string phoneNumber, string message, string recipientName, CancellationToken ct);
}

public record WhatsAppResult(bool Sent, string? WaMeLink, string? Error);
