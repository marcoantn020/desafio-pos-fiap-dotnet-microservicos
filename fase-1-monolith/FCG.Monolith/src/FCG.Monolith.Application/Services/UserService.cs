using FCG.Monolith.Application.Auth;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using FCG.Monolith.Domain.ValueObjects;

namespace FCG.Monolith.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> RegisterAsync(string name, string email, string password, CancellationToken ct = default)
    {
        var emailVo = Email.Create(email);
        Password.Validate(password);

        var existing = await _userRepository.GetByEmailAsync(emailVo.Value, ct);
        if (existing is not null)
            throw new InvalidOperationException("Email already registered.");

        var passwordHash = _passwordHasher.Hash(password);
        var user = User.Create(name, emailVo.Value, passwordHash);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        var token = _tokenService.GenerateToken(user);
        return new AuthResult(token, user.Id, user.Name, user.Email, user.Role.ToString());
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant(), ct);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);
        return new AuthResult(token, user.Id, user.Name, user.Email, user.Role.ToString());
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => _userRepository.GetAllAsync(ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _userRepository.GetByIdAsync(id, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        await _userRepository.DeleteAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
    }
}
