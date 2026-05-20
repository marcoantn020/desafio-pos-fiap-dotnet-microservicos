using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Infrastructure.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
}
