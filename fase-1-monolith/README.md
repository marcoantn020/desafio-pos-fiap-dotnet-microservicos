# FCG Phase 1 — Monolith API

FIAP Cloud Games Phase 1 MVP. .NET 8 monolith Web API com gerenciamento de usuários, autenticação JWT, catálogo de jogos, biblioteca pessoal e promoções.

## Project Structure

```
FCG.Monolith.Domain         — Entities, Value Objects, Repository Interfaces (sem dependências externas)
FCG.Monolith.Application    — Services, DTOs, Auth interfaces (Clean Architecture — Application Layer)
FCG.Monolith.Infrastructure — EF Core + PostgreSQL, JWT, BCrypt, Repository implementations
FCG.Monolith.API            — Controllers (thin), Middleware, Swagger
FCG.Monolith.Tests          — xUnit domain unit tests (TDD — 51 testes)
```

## Prerequisites

- .NET 8 SDK
- Docker (para PostgreSQL)
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

Migrations são aplicadas automaticamente no startup. Um usuário Admin é criado na primeira execução.

3. Swagger UI: http://localhost:5000/swagger

## Default Credentials

| Role  | Email         | Password   |
|-------|---------------|------------|
| Admin | admin@fcg.com | Admin@1234 |

## API Endpoints

### Auth (sem token)
| Method | Path                  | Description           |
|--------|-----------------------|-----------------------|
| POST   | /api/auth/register    | Registra novo usuário |
| POST   | /api/auth/login       | Login → retorna JWT   |

### Games
| Method | Path            | Auth  | Description           |
|--------|-----------------|-------|-----------------------|
| GET    | /api/games      | —     | Lista todos os jogos  |
| GET    | /api/games/{id} | —     | Jogo por ID           |
| POST   | /api/games      | Admin | Cadastra jogo         |
| PUT    | /api/games/{id} | Admin | Atualiza jogo         |
| DELETE | /api/games/{id} | Admin | Remove jogo           |

### Library (Biblioteca pessoal)
| Method | Path                  | Auth | Description               |
|--------|-----------------------|------|---------------------------|
| GET    | /api/library          | User | Minha biblioteca de jogos |
| POST   | /api/library/{gameId} | User | Adiciona jogo à biblioteca |

### Promotions
| Method | Path                               | Auth  | Description                    |
|--------|------------------------------------|-------|--------------------------------|
| GET    | /api/promotions                    | —     | Lista promoções ativas         |
| GET    | /api/promotions/{id}               | —     | Promoção por ID (com jogos)    |
| POST   | /api/promotions                    | Admin | Cria promoção                  |
| PUT    | /api/promotions/{id}               | Admin | Atualiza promoção              |
| DELETE | /api/promotions/{id}               | Admin | Remove promoção                |
| POST   | /api/promotions/{id}/games/{gameId}| Admin | Adiciona jogo à promoção       |
| DELETE | /api/promotions/{id}/games/{gameId}| Admin | Remove jogo da promoção        |

### Users (Admin only)
| Method | Path            | Description        |
|--------|-----------------|--------------------|
| GET    | /api/users      | Lista usuários     |
| GET    | /api/users/{id} | Usuário por ID     |
| DELETE | /api/users/{id} | Remove usuário     |

## Running Tests

```bash
dotnet test fase-1-monolith/FCG.Monolith/FCG.Monolith.sln
```

Saída esperada: **51 testes aprovados, 0 falhas**.

## Password Policy

Mínimo 8 caracteres com pelo menos: uma letra maiúscula, uma minúscula, um número e um caractere especial (ex: `Valid@123`).

## DDD Documentation

Documentação DDD em `docs/ddd/`:

- `event-storming-user-registration.drawio` — fluxo de cadastro de usuário
- `event-storming-game-creation.drawio` — fluxo de criação de jogo (Admin)
- `event-storming-library.drawio` — fluxo de aquisição de jogo
- `context-map.drawio` — Context Map com os 4 bounded contexts
- `README.md` — glossário ubíquo e descrição dos diagramas

Para abrir os `.drawio`: acesse [app.diagrams.net](https://app.diagrams.net) → File → Open from → Device.

## Architecture Notes

Monolito Clean Architecture seguindo princípios DDD:

- **Domain** — regras de negócio puras, sem dependências externas
- **Application** — orquestra casos de uso via services; define interfaces (`ITokenService`, `IPasswordHasher`)
- **Infrastructure** — implementações concretas (EF Core, BCrypt, JWT)
- **API** — controllers finos que delegam para Application

Em Phase 2, o monolito foi dividido em microserviços: UsersAPI, CatalogAPI, PaymentsAPI, NotificationsAPI.
