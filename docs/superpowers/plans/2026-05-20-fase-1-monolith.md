# FCG Phase 1 — Monolith API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET 8 monolith Web API for FIAP Cloud Games Phase 1 with user registration, JWT auth (User/Admin roles), game catalog, user library, EF Core + PostgreSQL, Swagger, error-handling middleware, and domain unit tests (TDD).

**Architecture:** Three-project solution: `FCG.Monolith.Domain` (entities, value objects, interfaces — no external deps), `FCG.Monolith.Infrastructure` (EF Core, repositories, JWT), `FCG.Monolith.API` (controllers, middleware, DTOs, Swagger). All in `fase-1-monolith/FCG.Monolith/`. Tests in a separate `FCG.Monolith.Tests` project.

**Tech Stack:** .NET 8, ASP.NET Core MVC Controllers, EF Core 8 + Npgsql (PostgreSQL), Microsoft.AspNetCore.Authentication.JwtBearer 8, BCrypt.Net-Next 4, Swashbuckle.AspNetCore 6, xUnit, FluentAssertions

---

## File Map

```
fase-1-monolith/
  README.md
  FCG.Monolith/
    FCG.Monolith.sln
    src/
      FCG.Monolith.Domain/
        Enums/UserRole.cs
        Entities/User.cs
        Entities/Game.cs
        Entities/LibraryItem.cs
        ValueObjects/Email.cs
        ValueObjects/Password.cs
        Interfaces/IUserRepository.cs
        Interfaces/IGameRepository.cs
        Interfaces/ILibraryRepository.cs
      FCG.Monolith.Infrastructure/
        DependencyInjection.cs
        Auth/ITokenService.cs
        Auth/JwtTokenService.cs
        Persistence/AppDbContext.cs
        Persistence/Configurations/UserConfiguration.cs
        Persistence/Configurations/GameConfiguration.cs
        Persistence/Configurations/LibraryItemConfiguration.cs
        Persistence/Repositories/UserRepository.cs
        Persistence/Repositories/GameRepository.cs
        Persistence/Repositories/LibraryRepository.cs
        Persistence/Migrations/  (auto-generated)
      FCG.Monolith.API/
        Program.cs
        appsettings.json
        appsettings.Development.json
        Middleware/ErrorHandlingMiddleware.cs
        DTOs/Auth/RegisterRequest.cs
        DTOs/Auth/LoginRequest.cs
        DTOs/Auth/AuthResponse.cs
        DTOs/Games/CreateGameRequest.cs
        DTOs/Games/UpdateGameRequest.cs
        DTOs/Games/GameResponse.cs
        DTOs/Users/UserResponse.cs
        DTOs/Library/LibraryItemResponse.cs
        Controllers/AuthController.cs
        Controllers/GamesController.cs
        Controllers/LibraryController.cs
        Controllers/UsersController.cs
    tests/
      FCG.Monolith.Tests/
        Domain/EmailTests.cs
        Domain/PasswordTests.cs
        Domain/UserEntityTests.cs
        Domain/GameEntityTests.cs
```

---

### Task 0: Create branch and solution scaffold

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/FCG.Monolith.sln`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/FCG.Monolith.API.csproj`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj`
- Create: `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj`

- [ ] **Step 1: Create git branch**

```bash
git checkout -b fase-1
```
Expected: `Switched to a new branch 'fase-1'`

- [ ] **Step 2: Create solution and projects**

Run from the repo root (`/home/marco/POS-FIAP/desafio-pos-fiap-dotnet-microservicos`):

```bash
mkdir -p fase-1-monolith/FCG.Monolith
cd fase-1-monolith/FCG.Monolith
dotnet new sln -n FCG.Monolith
dotnet new webapi -n FCG.Monolith.API --no-openapi false -o src/FCG.Monolith.API
dotnet new classlib -n FCG.Monolith.Domain -o src/FCG.Monolith.Domain
dotnet new classlib -n FCG.Monolith.Infrastructure -o src/FCG.Monolith.Infrastructure
dotnet new xunit -n FCG.Monolith.Tests -o tests/FCG.Monolith.Tests
```
Expected: 5 "The template ... was created successfully." messages.

- [ ] **Step 3: Add projects to solution**

```bash
dotnet sln add src/FCG.Monolith.API/FCG.Monolith.API.csproj
dotnet sln add src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
dotnet sln add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
dotnet sln add tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj
```

- [ ] **Step 4: Add project references**

```bash
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj reference src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj reference src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj reference src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
dotnet add tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj reference src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
dotnet add tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj reference src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
```

- [ ] **Step 5: Install NuGet packages**

```bash
# Infrastructure
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package BCrypt.Net-Next --version 4.0.3
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt --version 7.5.0
dotnet add src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

# API
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj package Swashbuckle.AspNetCore --version 6.5.0
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add src/FCG.Monolith.API/FCG.Monolith.API.csproj package BCrypt.Net-Next --version 4.0.3

# Tests
dotnet add tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj package Moq --version 4.20.70
dotnet add tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj package FluentAssertions --version 6.12.0
```

- [ ] **Step 6: Verify build**

```bash
dotnet build FCG.Monolith.sln
```
Expected: `Build succeeded.`

- [ ] **Step 7: Commit scaffold**

```bash
cd /home/marco/POS-FIAP/desafio-pos-fiap-dotnet-microservicos
git add fase-1-monolith/
git commit -m "feat(fase-1): scaffold solution with Domain, Infrastructure, API, and Tests projects"
```

---

### Task 1: Domain — Enums and Entities

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Enums/UserRole.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/User.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/Game.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/LibraryItem.cs`

- [ ] **Step 1: Remove generated Class1.cs**

```bash
rm fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Class1.cs
```

- [ ] **Step 2: Create UserRole enum**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Enums/UserRole.cs`:

```csharp
namespace FCG.Monolith.Domain.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}
```

- [ ] **Step 3: Create User entity**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/User.cs`:

```csharp
using FCG.Monolith.Domain.Enums;

namespace FCG.Monolith.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<LibraryItem> Library { get; private set; } = new List<LibraryItem>();

    private User() { }

    public static User Create(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Name = name.Trim();
    }

    public bool IsAdmin() => Role == UserRole.Admin;
}
```

- [ ] **Step 4: Create Game entity**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/Game.cs`:

```csharp
namespace FCG.Monolith.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Genre { get; private set; } = string.Empty;
    public int ReleaseYear { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Game() { }

    public static Game Create(string title, string description, decimal price, string genre, int releaseYear)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (releaseYear < 1970 || releaseYear > DateTime.UtcNow.Year + 2)
            throw new ArgumentException("Invalid release year.", nameof(releaseYear));

        return new Game
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Price = price,
            Genre = genre?.Trim() ?? string.Empty,
            ReleaseYear = releaseYear,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, decimal price, string genre, int releaseYear)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Genre = genre?.Trim() ?? string.Empty;
        ReleaseYear = releaseYear;
    }
}
```

- [ ] **Step 5: Create LibraryItem entity**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Entities/LibraryItem.cs`:

```csharp
namespace FCG.Monolith.Domain.Entities;

public class LibraryItem
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredAt { get; private set; }
    public User User { get; private set; } = null!;
    public Game Game { get; private set; } = null!;

    private LibraryItem() { }

    public static LibraryItem Create(Guid userId, Guid gameId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (gameId == Guid.Empty) throw new ArgumentException("GameId is required.", nameof(gameId));
        return new LibraryItem
        {
            UserId = userId,
            GameId = gameId,
            AcquiredAt = DateTime.UtcNow
        };
    }
}
```

- [ ] **Step 6: Build Domain**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 7: Commit domain entities**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/
git commit -m "feat(fase-1): add User, Game, LibraryItem domain entities and UserRole enum"
```

---

### Task 2: Domain — Value Objects (TDD)

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/EmailTests.cs`
- Create: `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/PasswordTests.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/ValueObjects/Email.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/ValueObjects/Password.cs`

- [ ] **Step 1: Write failing test for Email (TDD — Red)**

Create `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/EmailTests.cs`:

```csharp
using FCG.Monolith.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@example.co.uk")]
    public void Create_ValidEmail_ReturnsLowercaseValue(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Create_InvalidEmail_ThrowsArgumentException(string input)
    {
        var act = () => Email.Create(input);
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 2: Write failing test for Password (TDD — Red)**

Create `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/PasswordTests.cs`:

```csharp
using FCG.Monolith.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class PasswordTests
{
    [Theory]
    [InlineData("Valid@123")]
    [InlineData("Abc!1234")]
    [InlineData("Str0ng#Password")]
    public void Validate_ValidPassword_DoesNotThrow(string password)
    {
        var act = () => Password.Validate(password);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("NoNumber@Abc")]
    public void Validate_InvalidPassword_ThrowsArgumentException(string password)
    {
        var act = () => Password.Validate(password);
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 3: Run tests — confirm build fails (Red confirmed)**

```bash
dotnet build fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj 2>&1 | tail -5
```
Expected: Build error — `Email` and `Password` not found.

- [ ] **Step 4: Implement Email value object (TDD — Green)**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/ValueObjects/Email.cs`:

```csharp
using System.Text.RegularExpressions;

namespace FCG.Monolith.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        return new Email(normalized);
    }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}
```

- [ ] **Step 5: Implement Password value object (TDD — Green)**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/ValueObjects/Password.cs`:

```csharp
using System.Text.RegularExpressions;

namespace FCG.Monolith.Domain.ValueObjects;

public static class Password
{
    // Min 8 chars, one uppercase, one lowercase, one digit, one special char
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        RegexOptions.Compiled);

    public static void Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        if (!PasswordRegex.IsMatch(password))
            throw new ArgumentException(
                "Password must be at least 8 characters and contain uppercase, lowercase, digit, and special character.",
                nameof(password));
    }
}
```

- [ ] **Step 6: Run tests — confirm they pass (TDD — Green)**

```bash
dotnet test fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj --filter "EmailTests|PasswordTests" -v minimal
```
Expected: All tests PASS.

- [ ] **Step 7: Commit value objects with tests**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/ValueObjects/ fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/
git commit -m "feat(fase-1): add Email and Password value objects using TDD"
```

---

### Task 3: Domain — Repository Interfaces

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/IUserRepository.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/IGameRepository.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/ILibraryRepository.cs`

- [ ] **Step 1: Create IUserRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/IUserRepository.cs`:

```csharp
using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 2: Create IGameRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/IGameRepository.cs`:

```csharp
using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Game game, CancellationToken ct = default);
    Task DeleteAsync(Game game, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 3: Create ILibraryRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/ILibraryRepository.cs`:

```csharp
using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Domain.Interfaces;

public interface ILibraryRepository
{
    Task<IEnumerable<LibraryItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<LibraryItem?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task AddAsync(LibraryItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 4: Build Domain**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/FCG.Monolith.Domain.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit interfaces**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Domain/Interfaces/
git commit -m "feat(fase-1): add repository interfaces to domain"
```

---

### Task 4: Infrastructure — EF Core DbContext and Entity Configurations

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/AppDbContext.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/GameConfiguration.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/LibraryItemConfiguration.cs`

- [ ] **Step 1: Remove generated Class1.cs**

```bash
rm fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Class1.cs
```

- [ ] **Step 2: Create AppDbContext**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 3: Create UserConfiguration**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/UserConfiguration.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
```

- [ ] **Step 4: Create GameConfiguration**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/GameConfiguration.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).IsRequired().HasMaxLength(200);
        builder.HasIndex(g => g.Title).IsUnique();
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Price).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(g => g.Genre).HasMaxLength(100);
        builder.Property(g => g.ReleaseYear).IsRequired();
        builder.Property(g => g.CreatedAt).IsRequired();
    }
}
```

- [ ] **Step 5: Create LibraryItemConfiguration**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Configurations/LibraryItemConfiguration.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Monolith.Infrastructure.Persistence.Configurations;

public class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.HasKey(li => new { li.UserId, li.GameId });

        builder.HasOne(li => li.User)
            .WithMany(u => u.Library)
            .HasForeignKey(li => li.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.Game)
            .WithMany()
            .HasForeignKey(li => li.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(li => li.AcquiredAt).IsRequired();
    }
}
```

- [ ] **Step 6: Build Infrastructure**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 7: Commit persistence layer**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/
git commit -m "feat(fase-1): add EF Core AppDbContext and entity configurations"
```

---

### Task 5: Infrastructure — Repositories

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/UserRepository.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/GameRepository.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/LibraryRepository.cs`

- [ ] **Step 1: Create UserRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/UserRepository.cs`:

```csharp
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
```

- [ ] **Step 2: Create GameRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/GameRepository.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context) => _context = context;

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Games.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default)
        => await _context.Games.OrderBy(g => g.Title).ToListAsync(ct);

    public async Task AddAsync(Game game, CancellationToken ct = default)
        => await _context.Games.AddAsync(game, ct);

    public Task DeleteAsync(Game game, CancellationToken ct = default)
    {
        _context.Games.Remove(game);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
```

- [ ] **Step 3: Create LibraryRepository**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/LibraryRepository.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Monolith.Infrastructure.Persistence.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly AppDbContext _context;

    public LibraryRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<LibraryItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.LibraryItems
            .Include(li => li.Game)
            .Where(li => li.UserId == userId)
            .ToListAsync(ct);

    public Task<LibraryItem?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default)
        => _context.LibraryItems
            .FirstOrDefaultAsync(li => li.UserId == userId && li.GameId == gameId, ct);

    public async Task AddAsync(LibraryItem item, CancellationToken ct = default)
        => await _context.LibraryItems.AddAsync(item, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
```

- [ ] **Step 4: Build Infrastructure**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit repositories**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Persistence/Repositories/
git commit -m "feat(fase-1): add EF Core repository implementations"
```

---

### Task 6: Infrastructure — JWT Token Service and DI

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Auth/ITokenService.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Auth/JwtTokenService.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create ITokenService**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Auth/ITokenService.cs`:

```csharp
using FCG.Monolith.Domain.Entities;

namespace FCG.Monolith.Infrastructure.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
}
```

- [ ] **Step 2: Create JwtTokenService**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Auth/JwtTokenService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG.Monolith.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FCG.Monolith.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    public string GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
        var issuer = jwtSection["Issuer"] ?? "fcg.users";
        var audience = jwtSection["Audience"] ?? "fcg";
        var expiresInHours = int.Parse(jwtSection["ExpiresInHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("displayName", user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresInHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Step 3: Create DependencyInjection extension**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/DependencyInjection.cs`:

```csharp
using FCG.Monolith.Domain.Interfaces;
using FCG.Monolith.Infrastructure.Auth;
using FCG.Monolith.Infrastructure.Persistence;
using FCG.Monolith.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Monolith.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
```

- [ ] **Step 4: Build Infrastructure**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit JWT and DI**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/Auth/ fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/DependencyInjection.cs
git commit -m "feat(fase-1): add JWT token service and infrastructure DI registration"
```

---

### Task 7: API — DTOs and Error-Handling Middleware

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/RegisterRequest.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/LoginRequest.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/AuthResponse.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/CreateGameRequest.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/UpdateGameRequest.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/GameResponse.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Users/UserResponse.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Library/LibraryItemResponse.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Middleware/ErrorHandlingMiddleware.cs`

- [ ] **Step 1: Create Auth DTOs**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/RegisterRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Auth;

public record RegisterRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    [Required] string Password
);
```

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/LoginRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Auth;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
```

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Auth/AuthResponse.cs`:

```csharp
namespace FCG.Monolith.API.DTOs.Auth;

public record AuthResponse(
    string Token,
    string UserId,
    string Name,
    string Email,
    string Role
);
```

- [ ] **Step 2: Create Game DTOs**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/CreateGameRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Games;

public record CreateGameRequest(
    [Required] string Title,
    string? Description,
    [Required][Range(0, double.MaxValue)] decimal Price,
    string? Genre,
    [Required][Range(1970, 2100)] int ReleaseYear
);
```

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/UpdateGameRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace FCG.Monolith.API.DTOs.Games;

public record UpdateGameRequest(
    [Required] string Title,
    string? Description,
    [Required][Range(0, double.MaxValue)] decimal Price,
    string? Genre,
    [Required][Range(1970, 2100)] int ReleaseYear
);
```

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Games/GameResponse.cs`:

```csharp
namespace FCG.Monolith.API.DTOs.Games;

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Genre,
    int ReleaseYear,
    DateTime CreatedAt
);
```

- [ ] **Step 3: Create User and Library DTOs**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Users/UserResponse.cs`:

```csharp
namespace FCG.Monolith.API.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt
);
```

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/Library/LibraryItemResponse.cs`:

```csharp
namespace FCG.Monolith.API.DTOs.Library;

public record LibraryItemResponse(
    Guid GameId,
    string Title,
    string Genre,
    decimal Price,
    DateTime AcquiredAt
);
```

- [ ] **Step 4: Create Error Handling Middleware**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Middleware/ErrorHandlingMiddleware.cs`:

```csharp
using System.Net;
using System.Text.Json;

namespace FCG.Monolith.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "An unexpected error occurred." }));
        }
    }
}
```

- [ ] **Step 5: Build API**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/FCG.Monolith.API.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 6: Commit DTOs and middleware**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/DTOs/ fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Middleware/
git commit -m "feat(fase-1): add API DTOs and error handling middleware"
```

---

### Task 8: API — Controllers

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/AuthController.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/GamesController.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/LibraryController.cs`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/UsersController.cs`

- [ ] **Step 1: Create AuthController**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/AuthController.cs`:

```csharp
using FCG.Monolith.API.DTOs.Auth;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using FCG.Monolith.Domain.ValueObjects;
using FCG.Monolith.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var email = Email.Create(request.Email);
        Password.Validate(request.Password);

        var existing = await _userRepository.GetByEmailAsync(email.Value, ct);
        if (existing is not null)
            return Conflict(new { error = "Email already registered." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, email.Value, passwordHash);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        var token = _tokenService.GenerateToken(user);
        return Created($"/api/users/{user.Id}",
            new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id.ToString(), user.Name, user.Email, user.Role.ToString()));
    }
}
```

- [ ] **Step 2: Create GamesController**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/GamesController.cs`:

```csharp
using FCG.Monolith.API.DTOs.Games;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameRepository _gameRepository;

    public GamesController(IGameRepository gameRepository) => _gameRepository = gameRepository;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var games = await _gameRepository.GetAllAsync(ct);
        return Ok(games.Select(g => new GameResponse(g.Id, g.Title, g.Description, g.Price, g.Genre, g.ReleaseYear, g.CreatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var game = Game.Create(request.Title, request.Description ?? string.Empty, request.Price, request.Genre ?? string.Empty, request.ReleaseYear);
        await _gameRepository.AddAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
        return Created($"/api/games/{game.Id}",
            new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        game.Update(request.Title, request.Description ?? string.Empty, request.Price, request.Genre ?? string.Empty, request.ReleaseYear);
        await _gameRepository.SaveChangesAsync(ct);
        return Ok(new GameResponse(game.Id, game.Title, game.Description, game.Price, game.Genre, game.ReleaseYear, game.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct);
        if (game is null) return NotFound();
        await _gameRepository.DeleteAsync(game, ct);
        await _gameRepository.SaveChangesAsync(ct);
        return NoContent();
    }
}
```

- [ ] **Step 3: Create LibraryController**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/LibraryController.cs`:

```csharp
using System.Security.Claims;
using FCG.Monolith.API.DTOs.Library;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IGameRepository _gameRepository;

    public LibraryController(ILibraryRepository libraryRepository, IGameRepository gameRepository)
    {
        _libraryRepository = libraryRepository;
        _gameRepository = gameRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLibrary(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _libraryRepository.GetByUserIdAsync(userId, ct);
        return Ok(items.Select(li => new LibraryItemResponse(li.GameId, li.Game.Title, li.Game.Genre, li.Game.Price, li.AcquiredAt)));
    }

    [HttpPost("{gameId:guid}")]
    public async Task<IActionResult> AddToLibrary(Guid gameId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game is null) return NotFound(new { error = "Game not found." });

        var existing = await _libraryRepository.GetAsync(userId, gameId, ct);
        if (existing is not null) return Conflict(new { error = "Game already in library." });

        var item = LibraryItem.Create(userId, gameId);
        await _libraryRepository.AddAsync(item, ct);
        await _libraryRepository.SaveChangesAsync(ct);

        return Created("/api/library",
            new LibraryItemResponse(gameId, game.Title, game.Genre, game.Price, item.AcquiredAt));
    }
}
```

- [ ] **Step 4: Create UsersController**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/UsersController.cs`:

```csharp
using FCG.Monolith.API.DTOs.Users;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return Ok(users.Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role.ToString(), u.CreatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        return Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        await _userRepository.DeleteAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
        return NoContent();
    }
}
```

- [ ] **Step 5: Build API**

```bash
dotnet build fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/FCG.Monolith.API.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 6: Commit controllers**

```bash
git add fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Controllers/
git commit -m "feat(fase-1): add Auth, Games, Library, and Users controllers"
```

---

### Task 9: API — Program.cs and appsettings

**Files:**
- Modify: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Program.cs`
- Modify: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/appsettings.json`
- Create: `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/appsettings.Development.json`

- [ ] **Step 1: Replace Program.cs**

Replace the contents of `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/Program.cs`:

```csharp
using System.Text;
using FCG.Monolith.API.Middleware;
using FCG.Monolith.Domain.Enums;
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Interfaces;
using FCG.Monolith.Infrastructure;
using FCG.Monolith.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FCG Phase 1 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedAdminAsync(scope.ServiceProvider);
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static async Task SeedAdminAsync(IServiceProvider services)
{
    var userRepo = services.GetRequiredService<IUserRepository>();
    var config = services.GetRequiredService<IConfiguration>();

    var adminEmail = config["Seed:AdminEmail"] ?? "admin@fcg.com";
    if (await userRepo.GetByEmailAsync(adminEmail) is not null) return;

    var adminPassword = config["Seed:AdminPassword"] ?? "Admin@1234";
    var hash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
    var admin = User.Create("Admin FCG", adminEmail, hash, UserRole.Admin);
    await userRepo.AddAsync(admin);
    await userRepo.SaveChangesAsync();
}
```

- [ ] **Step 2: Update appsettings.json**

Replace `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/appsettings.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fcg_phase1;Username=fcg;Password=fcgpw"
  },
  "Jwt": {
    "SecretKey": "fcg-phase1-super-secret-key-minimum-32-characters-required!",
    "Issuer": "fcg.users",
    "Audience": "fcg",
    "ExpiresInHours": "24"
  },
  "Seed": {
    "AdminEmail": "admin@fcg.com",
    "AdminPassword": "Admin@1234"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Create appsettings.Development.json**

Create `fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fcg_phase1;Username=fcg;Password=fcgpw"
  }
}
```

- [ ] **Step 4: Build full solution**

```bash
dotnet build fase-1-monolith/FCG.Monolith/FCG.Monolith.sln
```
Expected: `Build succeeded.` (all 4 projects)

- [ ] **Step 5: Add EF migration**

First ensure `dotnet-ef` is installed:
```bash
dotnet tool install --global dotnet-ef 2>/dev/null || dotnet tool update --global dotnet-ef
```

Then generate the migration (run from the repo root):
```bash
dotnet ef migrations add InitialCreate \
  --project fase-1-monolith/FCG.Monolith/src/FCG.Monolith.Infrastructure/FCG.Monolith.Infrastructure.csproj \
  --startup-project fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/FCG.Monolith.API.csproj \
  --output-dir Persistence/Migrations
```
Expected: `Done. To undo this action, use 'ef migrations remove'`

- [ ] **Step 6: Commit**

```bash
git add fase-1-monolith/FCG.Monolith/src/
git commit -m "feat(fase-1): wire up Program.cs with JWT auth, Swagger, admin seed, and add InitialCreate migration"
```

---

### Task 10: Unit Tests — Domain Entities (TDD)

**Files:**
- Create: `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/UserEntityTests.cs`
- Create: `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/GameEntityTests.cs`

- [ ] **Step 1: Remove generated UnitTest1.cs**

```bash
rm fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/UnitTest1.cs
```

- [ ] **Step 2: Create UserEntityTests**

Create `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/UserEntityTests.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using FCG.Monolith.Domain.Enums;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsUserWithCorrectValues()
    {
        var user = User.Create("Alice", "alice@example.com", "hashedpw");

        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashedpw");
        user.Role.Should().Be(UserRole.User);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithAdminRole_SetsAdminAndIsAdminReturnsTrue()
    {
        var user = User.Create("Admin", "admin@example.com", "hashedpw", UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void Create_WithDefaultRole_IsAdminReturnsFalse()
    {
        var user = User.Create("User", "user@example.com", "hashedpw");
        user.IsAdmin().Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsArgumentException(string name)
    {
        var act = () => User.Create(name, "user@example.com", "hashedpw");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyPasswordHash_ThrowsArgumentException()
    {
        var act = () => User.Create("Name", "user@example.com", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        var user = User.Create("Old Name", "user@example.com", "hashedpw");
        user.UpdateName("New Name");
        user.Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsArgumentException()
    {
        var user = User.Create("Name", "user@example.com", "hashedpw");
        var act = () => user.UpdateName("");
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 3: Create GameEntityTests**

Create `fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/GameEntityTests.cs`:

```csharp
using FCG.Monolith.Domain.Entities;
using FluentAssertions;

namespace FCG.Monolith.Tests.Domain;

public class GameEntityTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsGameWithCorrectValues()
    {
        var game = Game.Create("Test Game", "A great game", 29.99m, "Action", 2024);

        game.Title.Should().Be("Test Game");
        game.Description.Should().Be("A great game");
        game.Price.Should().Be(29.99m);
        game.Genre.Should().Be("Action");
        game.ReleaseYear.Should().Be(2024);
        game.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyTitle_ThrowsArgumentException(string title)
    {
        var act = () => Game.Create(title, "desc", 9.99m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NegativePrice_ThrowsArgumentException()
    {
        var act = () => Game.Create("Game", "desc", -1m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ZeroPrice_Succeeds()
    {
        var act = () => Game.Create("Free Game", "desc", 0m, "RPG", 2024);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(1960)]
    [InlineData(1969)]
    public void Create_InvalidReleaseYear_ThrowsArgumentException(int year)
    {
        var act = () => Game.Create("Game", "desc", 9.99m, "RPG", year);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ValidInputs_UpdatesAllFields()
    {
        var game = Game.Create("Old Title", "Old desc", 9.99m, "RPG", 2020);
        game.Update("New Title", "New desc", 19.99m, "Action", 2024);

        game.Title.Should().Be("New Title");
        game.Description.Should().Be("New desc");
        game.Price.Should().Be(19.99m);
        game.Genre.Should().Be("Action");
        game.ReleaseYear.Should().Be(2024);
    }

    [Fact]
    public void Update_EmptyTitle_ThrowsArgumentException()
    {
        var game = Game.Create("Title", "desc", 9.99m, "RPG", 2024);
        var act = () => game.Update("", "desc", 9.99m, "RPG", 2024);
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 4: Run all tests**

```bash
dotnet test fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj -v minimal
```
Expected: All tests PASS (EmailTests + PasswordTests + UserEntityTests + GameEntityTests)

- [ ] **Step 5: Commit tests**

```bash
git add fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/UserEntityTests.cs fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/Domain/GameEntityTests.cs
git commit -m "test(fase-1): add TDD unit tests for User and Game domain entities"
```

---

### Task 11: README

**Files:**
- Create: `fase-1-monolith/README.md`

- [ ] **Step 1: Create README**

Create `fase-1-monolith/README.md`:

```markdown
# FCG Phase 1 — Monolith API

FIAP Cloud Games Phase 1 MVP. A .NET 8 monolith Web API with user management, JWT authentication, game catalog, and game library.

## Project Structure

```
FCG.Monolith.Domain       — Entities, Value Objects, Repository Interfaces (no external deps)
FCG.Monolith.Infrastructure — EF Core + PostgreSQL, JWT, Repository implementations
FCG.Monolith.API          — Controllers, Middleware, DTOs, Swagger
FCG.Monolith.Tests        — xUnit domain unit tests (TDD)
```

## Prerequisites

- .NET 8 SDK
- Docker (for PostgreSQL)
- EF Core tools: `dotnet tool install --global dotnet-ef`

## Running Locally

1. Start PostgreSQL (from the repo root):

```bash
cd infra && docker compose up -d
```

2. Run the API:

```bash
dotnet run --project fase-1-monolith/FCG.Monolith/src/FCG.Monolith.API/FCG.Monolith.API.csproj
```

Database migrations are applied automatically on startup. An Admin user is seeded on first run.

3. Open Swagger UI: http://localhost:5000/swagger

## Default Credentials

| Role  | Email         | Password   |
|-------|---------------|------------|
| Admin | admin@fcg.com | Admin@1234 |

## API Endpoints

### Auth (no token required)
| Method | Path               | Description           |
|--------|--------------------|-----------------------|
| POST   | /api/auth/register | Register new user     |
| POST   | /api/auth/login    | Login → returns JWT   |

### Games
| Method | Path            | Auth  | Description      |
|--------|-----------------|-------|------------------|
| GET    | /api/games      | —     | List all games   |
| GET    | /api/games/{id} | —     | Get game by ID   |
| POST   | /api/games      | Admin | Create game      |
| PUT    | /api/games/{id} | Admin | Update game      |
| DELETE | /api/games/{id} | Admin | Delete game      |

### Library
| Method | Path                  | Auth | Description          |
|--------|-----------------------|------|----------------------|
| GET    | /api/library          | User | Get my library       |
| POST   | /api/library/{gameId} | User | Add game to library  |

### Users (Admin only)
| Method | Path            | Description     |
|--------|-----------------|-----------------|
| GET    | /api/users      | List all users  |
| GET    | /api/users/{id} | Get user by ID  |
| DELETE | /api/users/{id} | Delete user     |

## Running Tests

```bash
dotnet test fase-1-monolith/FCG.Monolith/FCG.Monolith.sln
```

## Password Policy

Minimum 8 characters with at least one uppercase letter, one lowercase letter, one digit, and one special character (e.g. `Valid@123`).
```

- [ ] **Step 2: Final build and test check**

```bash
dotnet build fase-1-monolith/FCG.Monolith/FCG.Monolith.sln && dotnet test fase-1-monolith/FCG.Monolith/tests/FCG.Monolith.Tests/FCG.Monolith.Tests.csproj -v minimal
```
Expected: `Build succeeded.` followed by all tests PASS.

- [ ] **Step 3: Final commit**

```bash
git add fase-1-monolith/README.md
git commit -m "docs(fase-1): add README with setup instructions and API reference"
```
