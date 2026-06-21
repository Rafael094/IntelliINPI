namespace IntelliINPI.Application.Abstractions;

public interface INitNotificationService
{
    Task<string> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    Task<string> SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
