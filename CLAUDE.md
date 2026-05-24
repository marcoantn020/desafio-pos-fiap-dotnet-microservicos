# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**FCG (FIAP Cloud Games)** — a .NET 8 event-driven microservices system. Services communicate exclusively via RabbitMQ (MassTransit); there are no HTTP calls between services.

## Infrastructure

Start the required infrastructure (PostgreSQL + RabbitMQ) before running any service:

```bash
cd infra
docker compose up -d
```

- RabbitMQ Management UI: http://localhost:15672 (guest/guest)
- PostgreSQL: localhost:5432 (root: fcg/fcgpw)

Each service has its own database and dedicated DB user (see `infra/postgres/init/create-databases-01.sql`).

## Build & Run

Each service is an independent solution. Commands run from within each service's project directory:

```bash
# Build
dotnet build users-api/UsersAPI/UsersAPI.sln
dotnet build catalog-api/CatalogAPI/CatalogAPI.sln
dotnet build payments-api/PaymentsAPI/PaymentsAPI.sln
dotnet build notications-api/NotificationsAPI/NotificationsAPI.sln  # note: "notications" typo in folder name

# Run (from the .csproj directory)
dotnet run --project users-api/UsersAPI/UsersAPI/UsersAPI.csproj
dotnet run --project catalog-api/CatalogAPI/CatalogAPI/CatalogAPI.csproj
dotnet run --project payments-api/PaymentsAPI/PaymentsAPI/PaymentsAPI.csproj
dotnet run --project notications-api/NotificationsAPI/NotificationsAPI/NotificationsAPI.csproj
```

## Database Migrations

Migrations live inside each service project. To apply or add migrations, run from the project directory (where the `.csproj` is):

```bash
# Apply migrations
dotnet ef database update

# Add a new migration
dotnet ef migrations add <MigrationName>
```

Each service's `Program.cs` calls `db.Database.MigrateAsync()` on startup, so migrations are applied automatically when the service starts.

## Shared Contracts

`contracts/Contracts/Contracts/` contains the integration event records shared across all services:

- `UserCreatedEventV1` — published by UsersAPI
- `OrderPlacedEventV1` — published by CatalogAPI
- `PaymentProcessedEventV1` — published by PaymentsAPI

All events carry: `EventId` (Guid), `OccurredAtUtc`, `SchemaVersion` (int). The NuGet package ID is `FIAPCloudGames2026.Contracts`. When adding a breaking change to an event, create a new version (`V2`) rather than modifying the existing record.

## Architecture

### Services & Responsibilities

| Service | Publishes | Consumes |
|---------|-----------|---------|
| UsersAPI | `UserCreatedEventV1` | — |
| CatalogAPI | `OrderPlacedEventV1` | `PaymentProcessedEventV1` |
| PaymentsAPI | `PaymentProcessedEventV1` | `OrderPlacedEventV1` |
| NotificationsAPI | — | `UserCreatedEventV1`, `PaymentProcessedEventV1` |

### RabbitMQ Topology

| Exchange (topic) | Routing Key | Queue | Consumer |
|-----------------|-------------|-------|---------|
| `fcg.users` | `v1.user-created` | `notifications.user-created` | NotificationsAPI |
| `fcg.catalog` | `v1.order-placed` | `payments.order-placed` | PaymentsAPI |
| `fcg.payments` | `v1.payment-processed` | `catalog.payment-processed` | CatalogAPI |
| `fcg.payments` | `v1.payment-processed` | `notifications.payment-processed` | NotificationsAPI |

All consumers use `ConfigureConsumeTopology = false` and manually bind to exchanges with explicit routing keys. MassTransit auto-creates `<queue>_error` queues for failed messages after retries.

### Outbox Pattern

UsersAPI, CatalogAPI, and PaymentsAPI use **MassTransit EF Core Outbox** (`MassTransit.EntityFrameworkCore`). This makes the database write and event publish atomic — the event is first saved to `OutboxMessage` table within the same transaction, then a background job delivers it to RabbitMQ.

Each publishing service's `DbContext` maps three extra tables: `InboxState`, `OutboxMessage`, `OutboxState`.

NotificationsAPI has no outbox (consumer-only service).

### Idempotency

Consumers guard against duplicate delivery with:

- **Unique DB constraints**: `LibraryItems(UserId, GameId)` in CatalogAPI; `Payments.OrderId` in PaymentsAPI
- **InboxMessage table**: NotificationsAPI tracks `(EventId, ConsumerName)` before processing
- **Existence checks**: PaymentsAPI checks for an existing `Payment` record by `OrderId` before creating one

### JWT Authentication

Only UsersAPI issues tokens. CatalogAPI validates them using the same `Jwt` config section (`Issuer: fcg.users`, `Audience: fcg`). PaymentsAPI and NotificationsAPI have no HTTP auth — they only receive messages via RabbitMQ.

Token claims: `sub` (UserId as Guid), `email`, `displayName`, roles.

Admin endpoints in CatalogAPI (`POST/PUT/DELETE /games`) require the `Admin` role. Roles are seeded on UsersAPI startup.

### Payment Simulation

`PaymentsAPI` has no real payment integration. Approval is randomized based on `PaymentSimulation:ApprovalRatePercent` in `appsettings.json` (default: 80%).

---

## Phase 2 — Completed (2026-05-16)

### Separate GitHub Repositories

Each service lives in its own public repo under `marcoantn020`:

| Repo | URL |
|---|---|
| `fcg-contracts` | https://github.com/marcoantn020/fcg-contracts |
| `fcg-users-api` | https://github.com/marcoantn020/fcg-users-api |
| `fcg-catalog-api` | https://github.com/marcoantn020/fcg-catalog-api |
| `fcg-payments-api` | https://github.com/marcoantn020/fcg-payments-api |
| `fcg-notifications-api` | https://github.com/marcoantn020/fcg-notifications-api |
| `fcg-infra` | https://github.com/marcoantn020/fcg-infra |
| `fcg-frontend` | https://github.com/marcoantn020/fcg-frontend |

### Docker Hub Images

All service images published at `marcoantonio25/`:

```
marcoantonio25/fcg-users-api:latest
marcoantonio25/fcg-catalog-api:latest
marcoantonio25/fcg-payments-api:latest
marcoantonio25/fcg-notifications-api:latest
```

Each Dockerfile (in the individual repos) uses a multi-stage build that **clones `fcg-contracts` from GitHub** to build the NuGet package — making each repo fully self-contained.

### Kubernetes Manifests

`fcg-infra/k8s/` contains all manifests:
- `k8s/` — PostgreSQL (PVC, ConfigMap with init SQL, Deployment, Service) + RabbitMQ (Deployment + NodePort Service)
- `k8s/users-api/`, `k8s/catalog-api/`, `k8s/payments-api/`, `k8s/notifications-api/` — ConfigMap + Secret + Deployment + Service (NodePort)

NodePorts: users=30001, catalog=30002, payments=30003, notifications=30004, rabbitmq-mgmt=30015.

### Docker Compose (production)

`fcg-infra/docker-compose.yml` uses Docker Hub images (no `build:`). Run with:

```bash
git clone https://github.com/marcoantn020/fcg-infra
cd fcg-infra
docker compose up -d
```

Ports: users=5001, catalog=5002, payments=5003, notifications=5004, rabbitmq-mgmt=15672, postgres=5433.

### Game Seed Data

CatalogAPI has migration `20260516120000_SeedGames` with 25 games (fixed GUIDs `a1000000-0000-0000-0000-000000000001` through `...0025`). Applied automatically on startup.

### Frontend (Angular 17)

Located at `fcg-frontend/` in this monorepo and `https://github.com/marcoantn020/fcg-frontend`.

- `/login`, `/register` — auth via UsersAPI
- `/games` — catalog grid with "Comprar" button (calls CatalogAPI POST /orders)
- `/orders` — order list with status badges (Pending/Confirmed/Cancelled)
- `/library` — owned games (joins library + games endpoints)
- Auth guard + HTTP interceptor for JWT Bearer token
- Environment config at `src/environments/environment.ts` (usersApiUrl: 5001, catalogApiUrl: 5002)

```bash
cd fcg-frontend
npm install
ng serve   # http://localhost:4200
```

### Key Fixes Applied During Phase 2

- All `.csproj` files use `FIAPCloudGames2026.Contracts` (not old `SeuOrg.Contracts`)
- `OrderStatus` serialized as string via `JsonStringEnumConverter` in `ConfigureHttpJsonOptions`
- `db.Database.MigrateAsync()` added to all 4 services' `Program.cs` startup
- `ASPNETCORE_ENVIRONMENT: Development` set on users-api in docker-compose to trigger admin role seeding
