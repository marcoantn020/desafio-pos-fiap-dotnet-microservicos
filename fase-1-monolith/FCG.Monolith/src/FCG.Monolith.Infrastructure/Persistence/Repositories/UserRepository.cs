using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _context.Users.ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
