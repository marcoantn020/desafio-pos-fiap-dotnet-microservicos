# FCG Phase 1 — Monolith API

FIAP Cloud Games Phase 1 MVP. A .NET 8 monolith Web API with user management, JWT authentication, game catalog, and game library.

## Project Structure

```
FCG.Monolith.Domain         — Entities, Value Objects, Repository Interfaces (no external deps)
FCG.Monolith.Infrastructure — EF Core + PostgreSQL, JWT, Repository implementations
FCG.Monolith.API            — Controllers, Middleware, DTOs, Swagger
FCG.Monolith.Tests          — xUnit domain unit tests (TDD)
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
| Method | Path               | Description         |
|--------|--------------------|---------------------|
| POST   | /api/auth/register | Register new user   |
| POST   | /api/auth/login    | Login → returns JWT |

### Games
| Method | Path            | Auth  | Description    |
|--------|-----------------|-------|----------------|
| GET    | /api/games      | —     | List all games |
| GET    | /api/games/{id} | —     | Get game by ID |
| POST   | /api/games      | Admin | Create game    |
| PUT    | /api/games/{id} | Admin | Update game    |
| DELETE | /api/games/{id} | Admin | Delete game    |

### Library
| Method | Path                  | Auth | Description         |
|--------|-----------------------|------|---------------------|
| GET    | /api/library          | User | Get my library      |
| POST   | /api/library/{gameId} | User | Add game to library |

### Users (Admin only)
| Method | Path            | Description    |
|--------|-----------------|----------------|
| GET    | /api/users      | List all users |
| GET    | /api/users/{id} | Get user by ID |
| DELETE | /api/users/{id} | Delete user    |

## Running Tests

```bash
dotnet test fase-1-monolith/FCG.Monolith/FCG.Monolith.sln
```

## Password Policy

Minimum 8 characters with at least one uppercase letter, one lowercase letter, one digit, and one special character (e.g. `Valid@123`).

## Architecture Notes

This monolith follows DDD principles and is the Phase 1 MVP. In Phase 2, it evolves into microservices:
- **AuthController** → UsersAPI (same JWT config: Issuer `fcg.users`, Audience `fcg`)
- **GamesController + LibraryController** → CatalogAPI
- **Payments flow** → PaymentsAPI (added in Phase 2)
- **Notifications** → NotificationsAPI (added in Phase 2)
