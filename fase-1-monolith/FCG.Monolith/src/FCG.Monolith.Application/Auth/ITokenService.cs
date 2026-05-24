using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Application.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
}
