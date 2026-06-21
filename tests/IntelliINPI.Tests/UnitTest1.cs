using IntelliINPI.Application.Auth;
using IntelliINPI.Application.Trademarks;

namespace IntelliINPI.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void LoginValidator_ShouldRejectInvalidEmail()
    {
        var validator = new LoginCommandValidator();
        var result = validator.Validate(new LoginCommand("invalid", "password"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginRequest_ShouldAcceptSenhaAlias()
    {
        var request = new LoginRequest("admin@inpi.com", Senha: "temporary-password");

        Assert.Equal("temporary-password", request.EffectivePassword);
    }

    [Fact]
    public void SearchLocalValidator_ShouldRejectInvalidPageSize()
    {
        var validator = new SearchLocalTrademarksQueryValidator();
        var result = validator.Validate(new SearchLocalTrademarksQuery(null, null, null, null, 1, 500));

        Assert.False(result.IsValid);
    }
}
