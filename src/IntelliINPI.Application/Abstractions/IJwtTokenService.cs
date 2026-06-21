using IntelliINPI.Domain.Entities;

namespace IntelliINPI.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
