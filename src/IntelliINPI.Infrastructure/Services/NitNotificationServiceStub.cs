using IntelliINPI.Application.Abstractions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class NitNotificationServiceStub : INitNotificationService
{
    private const string Message = "Not implemented in local MVP";

    public Task<string> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }

    public Task<string> SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }
}
