using FCG.Monolith.Application.Auth;
using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Application.Services;

public interface IUserService
{
    Task<AuthResult> RegisterAsync(string name, string email, string password, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
