# FCG Fase 1 — Completar Requisitos Faltantes

**Data:** 2026-05-24  
**Status:** Aprovado pelo usuário

## Contexto

O projeto FCG Fase 1 (monolith .NET 8) está estruturalmente correto, mas faltam três itens obrigatórios do Tech Challenge:

1. **Promoções** — funcionalidade exigida para o papel Admin ("criar promoções")
2. **Camada Application** — controllers acessam repositories diretamente; DDD exige service layer
3. **Documentação DDD** — Event Storming e Context Map (entregável obrigatório em Miro/equivalente)

---

## 1. Promoções — Modelo de Domínio

### Entidade `Promotion` (Aggregate Root)

```
Promotion
  Id            : Guid
  Title         : string (obrigatório)
  Description   : string
  DiscountPercent: int (1–100, validado no domínio)
  StartsAt      : DateTime (UTC)
  EndsAt        : DateTime (UTC, deve ser > StartsAt)
  CreatedAt     : DateTime (UTC)
  Games         : ICollection<PromotionGame>
```

Regras de domínio em `Promotion.Create()` e `Promotion.Update()`:
- `DiscountPercent` entre 1 e 100 → `ArgumentException` caso contrário
- `EndsAt > StartsAt` → `ArgumentException` caso contrário
- `Title` não pode ser vazio

### Value Object `PromotionGame` (join table)

```
PromotionGame
  PromotionId : Guid (FK)
  GameId      : Guid (FK)
  AddedAt     : DateTime (UTC)
```

- Unique constraint em `(PromotionId, GameId)`
- `PromotionGame.Create(Guid promotionId, Guid gameId)` com validação de Guids vazios

### Interface `IPromotionRepository` (em Domain)

```csharp
GetAllActiveAsync(CancellationToken) → IEnumerable<Promotion>
GetByIdAsync(Guid, CancellationToken) → Promotion?
AddAsync(Promotion, CancellationToken)
UpdateAsync(Promotion, CancellationToken)  // via SaveChangesAsync
DeleteAsync(Promotion, CancellationToken)
AddGameAsync(PromotionGame, CancellationToken)
RemoveGameAsync(PromotionGame, CancellationToken)
GetPromotionGameAsync(Guid promotionId, Guid gameId, CancellationToken) → PromotionGame?
SaveChangesAsync(CancellationToken)
```

### GameResponse — Preço promocional

`GameResponse` ganha campo `PromotionalPrice : decimal?`. Calculado como `Price * (1 - DiscountPercent/100)` quando existe promoção ativa para o jogo no momento da consulta.

---

## 2. Endpoints de Promoções

| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/api/promotions` | — | Lista promoções ativas (`StartsAt <= now <= EndsAt`) |
| GET | `/api/promotions/{id}` | — | Detalhe com lista de jogos |
| POST | `/api/promotions` | Admin | Cria promoção |
| PUT | `/api/promotions/{id}` | Admin | Atualiza promoção |
| DELETE | `/api/promotions/{id}` | Admin | Remove promoção |
| POST | `/api/promotions/{id}/games/{gameId}` | Admin | Adiciona jogo à promoção |
| DELETE | `/api/promotions/{id}/games/{gameId}` | Admin | Remove jogo da promoção |

---

## 3. Camada Application

### Novo projeto `FCG.Monolith.Application`

- Depende de `FCG.Monolith.Domain` (apenas)
- Referenciado por `FCG.Monolith.API`
- `FCG.Monolith.Infrastructure` continua referenciando apenas Domain

### Services e interfaces

```
IUserService
  RegisterAsync(name, email, password) → AuthResult
  LoginAsync(email, password) → AuthResult
  GetAllAsync() → IEnumerable<User>
  GetByIdAsync(Guid) → User?
  DeleteAsync(Guid)

IGameService
  GetAllAsync() → IEnumerable<GameDto>      // inclui PromotionalPrice
  GetByIdAsync(Guid) → GameDto?
  CreateAsync(title, desc, price, genre, year) → GameDto
  UpdateAsync(Guid, title, desc, price, genre, year) → GameDto
  DeleteAsync(Guid)

ILibraryService
  GetByUserIdAsync(Guid userId) → IEnumerable<LibraryItemDto>
  AddToLibraryAsync(Guid userId, Guid gameId) → LibraryItemDto

IPromotionService
  GetAllActiveAsync() → IEnumerable<PromotionDto>
  GetByIdAsync(Guid) → PromotionDto?
  CreateAsync(title, desc, discountPercent, startsAt, endsAt) → PromotionDto
  UpdateAsync(Guid, title, desc, discountPercent, startsAt, endsAt) → PromotionDto
  DeleteAsync(Guid)
  AddGameAsync(Guid promotionId, Guid gameId)
  RemoveGameAsync(Guid promotionId, Guid gameId)
```

`AuthResult` = `(Token: string, UserId: Guid, Name: string, Email: string, Role: string)`

### Nota: ITokenService

`ITokenService` atualmente está em `Infrastructure`. Para que `UserService` (em Application) o utilize sem criar dependência circular, a **interface** `ITokenService` deve ser movida para `Application`. A implementação `JwtTokenService` permanece em `Infrastructure`.

### Controllers refatorados

Cada controller injeta `IXxxService` em vez de `IXxxRepository` + `ITokenService`. Lógica de negócio migra para os services. Controllers ficam responsáveis apenas por: deserializar request, chamar service, serializar response, retornar HTTP status code correto.

---

## 4. Infrastructure — mudanças

- `PromotionRepository` implementa `IPromotionRepository`
- `PromotionConfiguration` (EF Fluent API): tabela `Promotions`, chave composta em `PromotionGames`
- Migration `AddPromotions` criada via `dotnet ef migrations add`
- `DependencyInjection.cs` registra `IPromotionRepository` + todos os `IXxxService`
- `FCG.Monolith.Application.csproj` adicionado à solution (`FCG.Monolith.sln`)
- `PromotionalPrice`: quando um jogo possui múltiplas promoções ativas, aplica o maior desconto disponível

---

## 5. Testes novos

- `PromotionEntityTests` — Create válido, DiscountPercent inválido (0, 101), EndsAt <= StartsAt, Title vazio
- `PromotionGameTests` — Create válido, Guid vazio

---

## 6. Documentação DDD (`/docs/ddd/`)

### Arquivos gerados

```
docs/ddd/
  README.md                               — glossário ubíquo + explicação dos diagramas
  event-storming-user-registration.drawio — fluxo de cadastro de usuário
  event-storming-game-creation.drawio     — fluxo de criação de jogo (Admin)
  event-storming-library.drawio           — fluxo de aquisição de jogo
  context-map.drawio                      — Context Map + Aggregates + Value Objects
```

### Convenção de cores (Event Storming padrão)

| Cor | Significado |
|-----|-------------|
| 🟠 Laranja | Domain Event |
| 🔵 Azul claro | Command |
| 🟡 Amarelo | Aggregate / Entity |
| 🟣 Roxo | Policy / Reação |
| 🩷 Rosa | External System / Actor |
| 🟢 Verde | Read Model / View |

### Conteúdo dos diagramas

**event-storming-user-registration:** `POST /register` → ValidateEmail → ValidatePassword → UserCreated → [Admin seed no startup]

**event-storming-game-creation:** `POST /games` (Admin) → ValidateRole → GameCreated → GameAddedToPromotion (opcional)

**event-storming-library:** `POST /library/{gameId}` → ValidateUser → CheckDuplicate → GameAddedToLibrary

**context-map:** Bounded contexts User, Game, Library, Promotion com seus Aggregates, Value Objects e relações.

---

## Ordem de implementação

1. Domain: `Promotion` + `PromotionGame` entities + `IPromotionRepository`
2. Application project: interfaces + service implementations
3. Infrastructure: `PromotionRepository` + `PromotionConfiguration` + migration + DI update
4. API: `PromotionsController` + refatorar controllers existentes + DTOs
5. Tests: `PromotionEntityTests` + `PromotionGameTests`
6. Docs: `/docs/ddd/` com DrawIO + README.md
