# FCG — Documentação DDD

Documentação de Domain-Driven Design (DDD) para o projeto **FCG (FIAP Cloud Games)** — monólito .NET 8.

---

## Linguagem Ubíqua (Ubiquitous Language)

| Termo | Definição |
|---|---|
| **User** | Usuário do sistema. Pode ter o papel `User` (acesso básico) ou `Admin` (acesso administrativo). Identificado unicamente pelo e-mail. |
| **Admin** | Papel especial de User que pode criar, editar e excluir jogos e promoções. Requer token JWT com a role `Admin`. |
| **Game** | Jogo disponível no catálogo. Possui título, descrição, preço, gênero e ano de lançamento. |
| **Library** | Coleção de jogos adquiridos por um User. Cada entrada é um `LibraryItem`. |
| **LibraryItem** | Associação entre um User e um Game que ele adquiriu. Não permite duplicatas (idempotência garantida por constraint único `(UserId, GameId)`). |
| **Promotion** | Campanha promocional com desconto percentual, título, descrição e período de vigência (StartsAt/EndsAt). |
| **PromotionGame** | Associação entre uma Promotion e um Game, registrando quando o jogo foi adicionado à promoção. |
| **Email** | Value Object que encapsula um endereço de e-mail e garante que o formato seja válido. |
| **Password** | Value Object que encapsula as regras de política de senha: mínimo 8 caracteres, letra maiúscula, minúscula, dígito e caractere especial. |
| **JWT** | JSON Web Token emitido pela rota `/api/auth/login` ou `/api/auth/register`. Contém as claims `sub` (UserId), `email`, `displayName` e `role`. |
| **Aggregate Root** | Entidade que serve como ponto de entrada para um agregado. Garante a consistência do cluster de objetos. |
| **Value Object** | Objeto imutável definido pelos seus atributos, sem identidade própria. |
| **Domain Event** | Fato que ocorreu no domínio — representa algo que aconteceu e é relevante para outros contextos. |
| **Bounded Context** | Limite explícito dentro do qual um modelo de domínio é definido e aplicável. |
| **Command** | Intenção de alterar o estado do sistema; pode ser aceito ou rejeitado. |
| **Policy** | Regra de negócio ou invariante que decide se um comando pode ser executado. |

---

## Bounded Contexts

### 1. User Management

Responsável por tudo relacionado à identidade, autenticação e autorização.

- Gerencia o ciclo de vida do `User` (criação, autenticação).
- Emite **JWT tokens** usados pelos demais contextos para autorização.
- Valida `Email` (formato) e `Password` (política de segurança) por meio de Value Objects antes de persistir.
- Endpoints: `POST /api/auth/register`, `POST /api/auth/login`.

### 2. Game Catalog

Responsável pelo catálogo de jogos disponíveis na plataforma.

- Gerencia o ciclo de vida do `Game` (CRUD restrito a Admins).
- Serve como fonte da verdade para os demais contextos que referenciam jogos por `GameId`.
- Endpoints (requer role Admin): `GET/POST/PUT/DELETE /api/games`.

### 3. Promotions

Responsável pela criação e gerenciamento de campanhas promocionais.

- Gerencia `Promotion` e a associação `PromotionGame`.
- Um Admin pode criar promoções com desconto percentual (1–100%) e vinculá-las a jogos existentes no catálogo.
- Endpoints (requer role Admin): `GET/POST/PUT/DELETE /api/promotions`, `POST /api/promotions/{id}/games/{gameId}`.

### 4. Library

Responsável pelo controle da biblioteca pessoal de jogos de cada usuário.

- Gerencia `LibraryItem`, que representa a posse de um jogo por um usuário.
- Garante idempotência via constraint único `(UserId, GameId)` no banco de dados.
- Endpoints (requer JWT válido): `GET /api/library`, `POST /api/library/{gameId}`.

---

## Event Storming

Os diagramas de Event Storming modelam os fluxos principais do domínio na notação padrão com cores semânticas:

| Cor | Elemento | Significado |
|---|---|---|
| Laranja | Evento de Domínio | Algo que ocorreu no domínio |
| Azul | Comando | Intenção de alterar o estado |
| Amarelo | Agregado | Cluster de objetos com consistência |
| Roxo (losango) | Política | Regra/invariante de negócio |
| Verde | Read Model | Visão de leitura resultante |
| Rosa | Ator | Quem inicia a ação |

### `event-storming-user-registration.drawio`

Modela o fluxo de **registro de usuário** (`POST /api/auth/register`):

1. O Ator (Usuário) aciona as Políticas de validação de `Email` e `Password`.
2. Se válido, o Comando `RegisterUser` é enviado ao Agregado `User`.
3. O Agregado persiste o usuário e emite o Evento `UserRegistered`.
4. O Read Model retorna o `JWT Token Response`.
5. Caminho de erro: `ValidationFailed` caso Email ou Password sejam inválidos.

### `event-storming-game-creation.drawio`

Modela dois fluxos relacionados ao contexto de Game Catalog e Promotions:

**Fluxo 1 — Criação de Jogo** (`POST /api/games`):
1. Admin passa pela Política `RequireAdminRole`.
2. Comando `CreateGame` é enviado ao Agregado `Game`.
3. Evento `GameCreated` atualiza o Read Model do catálogo.

**Fluxo 2 — Vincular Jogo à Promoção** (`POST /api/promotions/{id}/games/{gameId}`):
1. Admin passa pela Política `RequireAdminRole` e `GameMustExist`.
2. Comando `AddGameToPromotion` é enviado ao Agregado `Promotion`.
3. Cria a entidade `PromotionGame` e emite `GameAddedToPromotion`.

### `event-storming-library.drawio`

Modela o fluxo de **adição de jogo à biblioteca** (`POST /api/library/{gameId}`):

1. Usuário autenticado (JWT) aciona a Política `GameMustExist`.
2. Política `NoDuplicateInLibrary` verifica se o jogo já está na biblioteca.
3. Comando `AddGameToLibrary` cria o `LibraryItem`.
4. Evento `GameAddedToLibrary` atualiza o Read Model da biblioteca.
5. Caminhos de erro: `GameNotFound` (404) e `DuplicateLibraryEntry` (409).

---

## Aggregates & Value Objects

| Nome | Tipo | Bounded Context | Campos Principais |
|---|---|---|---|
| `User` | Aggregate Root | User Management | Id, Name, Email (VO), PasswordHash, Role, CreatedAt |
| `Game` | Aggregate Root | Game Catalog | Id, Title, Description, Price, Genre, ReleaseYear, CreatedAt |
| `Promotion` | Aggregate Root | Promotions | Id, Title, Description, DiscountPercent, StartsAt, EndsAt, CreatedAt |
| `LibraryItem` | Entity | Library | UserId (FK), GameId (FK), AcquiredAt — UK(UserId, GameId) |
| `PromotionGame` | Entity | Promotions | PromotionId (FK), GameId (FK), AddedAt |
| `Email` | Value Object | User Management | Value: string — Regra: formato `user@domain.com` |
| `Password` | Value Object | User Management | Hash: string — Regra: ≥8 chars, maiúscula + minúscula + dígito + especial |

---

## Como abrir os diagramas

Os arquivos `.drawio` podem ser abertos diretamente no navegador sem instalação:

1. Acesse **[app.diagrams.net](https://app.diagrams.net)** (draw.io).
2. Clique em **File → Open From → Device**.
3. Selecione o arquivo `.drawio` desejado nesta pasta.

Ou importe via URL do repositório:

1. Acesse **[app.diagrams.net](https://app.diagrams.net)**.
2. Clique em **Extras → Edit Diagram** e cole o conteúdo XML do arquivo.

### Arquivos disponíveis

| Arquivo | Conteúdo |
|---|---|
| `event-storming-user-registration.drawio` | Fluxo de registro de usuário com validação de Email e Password VOs |
| `event-storming-game-creation.drawio` | Criação de jogo pelo Admin e vinculação de jogo à promoção |
| `event-storming-library.drawio` | Adição de jogo à biblioteca do usuário com políticas de guarda |
| `context-map.drawio` | Mapa de contextos com os 4 Bounded Contexts e seus relacionamentos |
