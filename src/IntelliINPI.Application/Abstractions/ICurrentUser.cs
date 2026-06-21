namespace IntelliINPI.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
    string? IpAddress { get; }
}
