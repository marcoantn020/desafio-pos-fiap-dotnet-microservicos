# FCG Frontend

> Web client for **FIAP Cloud Games** — a digital game store built on .NET 8 microservices.

![Angular](https://img.shields.io/badge/Angular-17-dd0031?logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178c6?logo=typescript&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Overview

FCG Frontend is a single-page application that provides the user interface for the FIAP Cloud Games platform. It communicates directly with two backend APIs:

| API | Responsibility | Default URL |
|---|---|---|
| **UsersAPI** | Registration, login, JWT issuance | `http://localhost:5001` |
| **CatalogAPI** | Game catalog, order placement, library | `http://localhost:5002` |

Authentication is handled via **JWT Bearer tokens** — issued by UsersAPI and automatically attached to every CatalogAPI request through an HTTP interceptor.

---

## Tech Stack

- **Angular 17** — standalone components, lazy-loaded routes
- **RxJS** — `forkJoin` for parallel requests, reactive forms
- **Angular Router** — `CanActivate` guard for protected routes
- **SCSS** — dark gaming theme with neon blue/green accents, no external UI library

---

## Getting Started

### Prerequisites

- Node.js 18+
- npm 9+
- Backend services running (see [fcg-infra](https://github.com/marcoantn020/fcg-infra))

### Install & Run

```bash
# Clone
git clone https://github.com/marcoantn020/fcg-frontend.git
cd fcg-frontend

# Install dependencies
npm install

# Start development server
ng serve
```

Open **http://localhost:4200** — the app redirects to `/login` if not authenticated.

### Build for Production

```bash
ng build
# Output: dist/fcg-frontend/
```

### Environment Configuration

Edit `src/environments/environment.ts` to point to your backend URLs:

```ts
export const environment = {
  production: false,
  usersApiUrl: 'http://localhost:5001',
  catalogApiUrl: 'http://localhost:5002',
};
```

---

## Routes

| Route | Auth Required | Description |
|---|:---:|---|
| `/` | — | Redirects to `/games` (or `/login` if unauthenticated) |
| `/login` | No | Sign in with email and password |
| `/register` | No | Create a new account |
| `/games` | Yes | Browse the game catalog and purchase games |
| `/orders` | Yes | View all placed orders and their payment status |
| `/library` | Yes | View games confirmed and available to play |
| `/**` | — | Wildcard — redirects to `/games` |

---

## Site Map

```
FCG Frontend
│
├── (public)
│   ├── /login
│   │   └── Form: email + password → JWT token → redirect to /games
│   │
│   └── /register
│       └── Form: name + email + password → JWT token → redirect to /games
│
└── (protected — requires JWT)
    │
    ├── /games  ─────────────────────────── Catalog
    │   ├── Lists all available games (GET /games)
    │   ├── Shows title and price per card
    │   └── "Comprar" button → places order (POST /orders)
    │                           └── success toast links to /orders
    │
    ├── /orders  ────────────────────────── Orders
    │   ├── Lists user's orders (GET /orders)
    │   ├── Enriches with game title (GET /games — forkJoin)
    │   └── Status badge per order:
    │       ├── ⏳ Pending   — awaiting PaymentsAPI processing
    │       ├── ✓ Confirmed  — payment approved, game added to library
    │       └── ✗ Cancelled  — payment declined
    │
    └── /library  ───────────────────────── Library
        ├── Lists owned games (GET /library)
        ├── Enriches with game title + price (GET /games — forkJoin)
        └── Shows acquisition date per game
```

---

## Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── guards/
│   │   │   └── auth.guard.ts          # Redirects to /login if no JWT
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts    # Injects Bearer token on every request
│   │   ├── models/
│   │   │   ├── auth.model.ts          # LoginRequest, RegisterRequest, AuthResponse
│   │   │   ├── game.model.ts          # Game
│   │   │   └── order.model.ts         # Order, LibraryItem
│   │   └── services/
│   │       ├── auth.service.ts        # login(), register(), logout(), getToken()
│   │       └── catalog.service.ts     # getGames(), placeOrder(), getOrders(), getLibrary()
│   │
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login/                 # /login page
│   │   │   └── register/              # /register page
│   │   ├── games/
│   │   │   └── game-list/             # /games page
│   │   ├── orders/
│   │   │   └── order-list/            # /orders page
│   │   └── library/
│   │       └── library-list/          # /library page
│   │
│   ├── shared/
│   │   └── components/
│   │       └── navbar/                # Top navigation bar
│   │
│   ├── app.component.ts               # Root component (navbar + router-outlet)
│   ├── app.config.ts                  # provideRouter + provideHttpClient + interceptors
│   └── app.routes.ts                  # Route definitions with lazy loading
│
├── environments/
│   ├── environment.ts                 # Development API URLs
│   └── environment.prod.ts            # Production API URLs
│
└── styles.scss                        # Global dark theme base styles
```

---

## Backend Integration

The app consumes the following endpoints:

```
UsersAPI (localhost:5001)
  POST /auth/register   → { userId, email, displayName, accessToken }
  POST /auth/login      → { userId, email, displayName, accessToken }

CatalogAPI (localhost:5002)  [all require Authorization: Bearer <token>]
  GET  /games           → Game[]
  POST /orders          → { id, status }   (202 Accepted)
  GET  /orders          → Order[]
  GET  /library         → LibraryItem[]
```

Order status lifecycle (driven by backend events via RabbitMQ):

```
POST /orders  →  Pending  →  (PaymentsAPI processes)  →  Confirmed
                                                       →  Cancelled
```

---

## Related Repositories

| Repository | Description |
|---|---|
| [fcg-contracts](https://github.com/marcoantn020/fcg-contracts) | Shared integration events (NuGet) |
| [fcg-users-api](https://github.com/marcoantn020/fcg-users-api) | Auth & user management |
| [fcg-catalog-api](https://github.com/marcoantn020/fcg-catalog-api) | Games, orders, library |
| [fcg-payments-api](https://github.com/marcoantn020/fcg-payments-api) | Payment simulation |
| [fcg-notifications-api](https://github.com/marcoantn020/fcg-notifications-api) | Email notifications |
| [fcg-infra](https://github.com/marcoantn020/fcg-infra) | Docker Compose & Kubernetes manifests |

---

## License

MIT
