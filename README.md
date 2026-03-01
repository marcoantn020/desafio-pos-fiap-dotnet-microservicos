# FCG (FIAP Cloud Games) — Fluxo de Eventos (Event-Driven)

Este projeto utiliza arquitetura **orientada a eventos** com **RabbitMQ + MassTransit**.
Não há chamadas HTTP entre microsserviços: a integração acontece exclusivamente via **publish/subscribe**.

## Visão Geral

### Microsserviços
- **UsersAPI**
    - Cadastro e autenticação (JWT)
    - Publica evento `UserCreatedEventV1`
- **CatalogAPI**
    - Catálogo de jogos (CRUD)
    - Biblioteca do usuário
    - Criação e gerenciamento de pedidos (Order)
    - Publica `OrderPlacedEventV1`
    - Consome `PaymentProcessedEventV1`
- **PaymentsAPI**
    - Processa pedidos recebidos via evento
    - Simula pagamento (Approved/Rejected)
    - Publica `PaymentProcessedEventV1`
- **NotificationsAPI**
    - Consome eventos e simula envio de e-mail via logs
    - Consome `UserCreatedEventV1` e `PaymentProcessedEventV1`

---

## Padrão de Mensageria (RabbitMQ)

### Exchanges (topic)
Utilizamos exchanges “por contexto/serviço”:

- `fcg.users` (publicações do UsersAPI)
- `fcg.catalog` (publicações do CatalogAPI)
- `fcg.payments` (publicações do PaymentsAPI)

### Routing Keys (versionadas)
Os eventos são roteados por routing keys `v1.*`:

- `v1.user-created`
- `v1.order-placed`
- `v1.payment-processed`

> A versão (`v1`) faz parte do contrato. Alterações incompatíveis geram uma nova versão (`v2.*`).

### Filas (queues) por consumidor
Cada consumidor tem sua própria fila, por exemplo:

- `payments.order-placed`
- `catalog.payment-processed`
- `notifications.user-created`
- `notifications.payment-processed`

O MassTransit também cria automaticamente:
- `<fila>_error` (mensagens que falharam após retries)
- `<fila>_skipped` (mensagens ignoradas por topology/config)

---

## Fluxos de Eventos

## 1) Cadastro de Usuário (UsersAPI → NotificationsAPI)

### Passos
1. **UsersAPI** cria o usuário (ASP.NET Identity).
2. UsersAPI publica `UserCreatedEventV1` na exchange `fcg.users` com routing key `v1.user-created`.
3. **NotificationsAPI** consome o evento na fila `notifications.user-created`.
4. NotificationsAPI simula o envio de e-mail via log.

### Roteamento
- Exchange: `fcg.users` (topic)
- Routing Key: `v1.user-created`
- Queue: `notifications.user-created`

---

## 2) Compra de Jogo (CatalogAPI → PaymentsAPI → CatalogAPI + NotificationsAPI)

### Passos
1. **CatalogAPI** recebe a requisição de compra e cria um `Order` com status `Pending`.
2. CatalogAPI publica `OrderPlacedEventV1` na exchange `fcg.catalog` com routing key `v1.order-placed`.
3. **PaymentsAPI** consome o evento na fila `payments.order-placed`, processa o pagamento e decide:
    - `Approved` ou `Rejected`
4. PaymentsAPI publica `PaymentProcessedEventV1` na exchange `fcg.payments` com routing key `v1.payment-processed`.
5. **CatalogAPI** consome `PaymentProcessedEventV1` na fila `catalog.payment-processed`:
    - se `Approved`: marca `Order` como `Confirmed` e adiciona o jogo à biblioteca
    - se `Rejected`: marca `Order` como `Rejected/Failed`
6. **NotificationsAPI** consome `PaymentProcessedEventV1` na fila `notifications.payment-processed` e loga o e-mail:
    - compra aprovada ✅ ou rejeitada ❌

### Roteamento do pedido
- Exchange: `fcg.catalog` (topic)
- Routing Key: `v1.order-placed`
- Queue: `payments.order-placed`

### Roteamento do pagamento
- Exchange: `fcg.payments` (topic)
- Routing Key: `v1.payment-processed`
- Queues:
    - `catalog.payment-processed`
    - `notifications.payment-processed`

---

## Confiabilidade e Consistência

### Outbox Pattern (publicação confiável)
Os serviços que publicam eventos (CatalogAPI e PaymentsAPI) utilizam **Outbox** com EF Core (MassTransit EntityFramework Outbox).
Isso garante que:
- o estado no banco e a publicação do evento sejam consistentes
- eventos não sejam perdidos em caso de falha após commit

### Idempotência (evitar duplicidade)
Consumidores devem ser idempotentes, pois sistemas de mensageria podem entregar mensagens mais de uma vez.

Exemplos usados:
- **Inbox/ProcessedMessages** (grava `EventId` consumido)
- **Unique constraints** (ex: biblioteca com índice único `(UserId, GameId)`)
- Antes de inserir, verificar se o registro já existe

---

## Convenções do Contrato (Eventos)

Cada evento possui (mínimo):
- `eventId` (GUID)
- `occurredAtUtc` (DateTime UTC)
- `schemaVersion` (int)
- Payload com IDs necessários (ex: `orderId`, `userId`, `gameId`)

Eventos são **contratos de integração**:
- não expõem entidades internas
- são versionados (`v1`, `v2`, ...)
- devem ser retrocompatíveis (quando possível)

---

## Troubleshooting

### Fila `<queue>_error` apareceu
É comportamento esperado do MassTransit. Isso indica que:
- a mensagem falhou no consumer (exception)
- após retries, foi movida para a fila de erro

Verifique:
- logs do consumer
- payload e headers da mensagem na fila `_error` (RabbitMQ Management UI)

______________________________________________________________________________________________________________

```plaintext
                         ┌───────────────────────────────┐
                         │           RabbitMQ            │
                         └───────────────────────────────┘

  ┌───────────────────┐          (topic)            ┌────────────────────────┐
  │      UsersAPI     │  PUBLISH  exchange:         │   exchange: fcg.users  │
  │                   ├──────────►│  fcg.users      │  (type: topic)         │
  └───────────────────┘           └────────────────────────┬─────────────────┘
                         routing key: v1.user-created      │
                                                           │ bind v1.user-created
                                                           ▼
                                                ┌───────────────────────────────────┐
                                                │ queue: notifications.user-created │
                                                └───────────────────────────────────┘
                                                           │
                                                           ▼
                                                ┌───────────────────────────┐
                                                │      NotificationsAPI     │
                                                │  Consumer: UserCreated    │
                                                └───────────────────────────┘


  ┌───────────────────┐          (topic)            ┌──────────────────────────┐
  │     CatalogAPI    │  PUBLISH  exchange:        │ exchange: fcg.catalog    │
  │                   ├──────────►│  fcg.catalog    │ (type: topic)            │
  └───────────────────┘           └─────────────────────────┬──────────────────┘
                         routing key: v1.order-placed       │
                                                            │ bind v1.order-placed
                                                            ▼
                                                ┌──────────────────────────────┐
                                                │ queue: payments.order-placed │
                                                └──────────────────────────────┘
                                                            │
                                                            ▼
                                                ┌───────────────────────────┐
                                                │        PaymentsAPI        │
                                                │ Consumer: OrderPlaced     │
                                                └───────────────────────────┘


  ┌───────────────────┐          (topic)            ┌─────────────────────────┐
  │     PaymentsAPI   │  PUBLISH  exchange:         │ exchange: fcg.payments  │
  │                   ├──────────►│  fcg.payments   │ (type: topic)           │
  └───────────────────┘           └───────────────┬────────────────┬──────────┘
                         routing key: v1.payment-processed         │
                                         │                         │
                       bind v1.payment-processed                   │ bind v1.payment-processed
                                         ▼                         ▼
                         ┌──────────────────────────────────┐ ┌────────────────────────────────────────┐
                         │ queue: catalog.payment-processed │ │ queue: notifications.payment-processed │
                         └──────────────────────────────────┘ └────────────────────────────────────────┘
                                         │                         │
                                         ▼                         ▼
                         ┌────────────────────────────┐   ┌────────────────────────────┐
                         │        CatalogAPI          │   │      NotificationsAPI      │
                         │ Consumer: PaymentProcessed │   │ Consumer: PaymentProcessed │
                         └────────────────────────────┘   └────────────────────────────┘


Observações:
- Para binds manuais, os consumers usam:
  - ConfigureConsumeTopology = false
  - Bind(exchange, routingKey, ExchangeType=topic)
- O MassTransit cria automaticamente filas auxiliares:
  - <queue>_error (mensagens com exception após retries)
```

# Exemplo de Fluxo Completo (eventos encadeados)

Abaixo um exemplo de uma jornada completa: **cadastro → compra → pagamento → biblioteca + notificações**.

---

## 0) Contexto

| Campo | Valor |
|---|---|
| `userId` | `4d9de275-d990-42b9-9f5b-b52290ab3c1f` |
| `gameId` | `a1b2c3d4-e5f6-47a1-9b8c-123456789000` |
| `orderId` | `85461b76-b4ad-4b4c-a9ac-ecd609ee061e` |
| `paymentId` | `6780b99e-d755-44d9-8bfc-6643a494181a` |
| `correlationId` | `85461b76-b4ad-4b4c-a9ac-ecd609ee061e` |

---

## 1) UsersAPI publica `UserCreatedEventV1`

- **Exchange:** `fcg.users` (topic)
- **Routing key:** `v1.user-created`
- **Queue destino:** `notifications.user-created`

```json
{
  "messageId": "2f3b0000-4c28-00e0-ffc2-08de77c080e2",
  "correlationId": "2f3b0000-4c28-00e0-ffc2-08de77c080e2",
  "messageType": [
    "urn:message:Contracts.IntegrationEvents:UserCreatedEventV1"
  ],
  "message": {
    "eventId": "2f3b0000-4c28-00e0-ffc2-08de77c080e2",
    "occurredAtUtc": "2026-03-01T18:00:00Z",
    "userId": "4d9de275-d990-42b9-9f5b-b52290ab3c1f",
    "email": "user@fcg.com",
    "displayName": "John Player",
    "schemaVersion": 1
  }
}
```

✅ **NotificationsAPI** consome e loga:
- `[EMAIL] Bem-vindo(a) John Player!`

---

## 2) CatalogAPI publica `OrderPlacedEventV1`

- **Exchange:** `fcg.catalog` (topic)
- **Routing key:** `v1.order-placed`
- **Queue destino:** `payments.order-placed`

```json
{
  "messageId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
  "correlationId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
  "messageType": [
    "urn:message:Contracts.IntegrationEvents:OrderPlacedEventV1"
  ],
  "message": {
    "eventId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
    "occurredAtUtc": "2026-03-01T18:10:00Z",
    "orderId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
    "userId": "4d9de275-d990-42b9-9f5b-b52290ab3c1f",
    "gameId": "a1b2c3d4-e5f6-47a1-9b8c-123456789000",
    "price": 199.9,
    "currency": "BRL",
    "schemaVersion": 1
  }
}
```

✅ **PaymentsAPI** consome e processa o pagamento.

---

## 3) PaymentsAPI publica `PaymentProcessedEventV1` (Approved)

- **Exchange:** `fcg.payments` (topic)
- **Routing key:** `v1.payment-processed`
- **Queues destino:**
    - `catalog.payment-processed`
    - `notifications.payment-processed`

```json
{
  "messageId": "9f7bec32-e63c-4c0b-ae0d-d6d57011287f",
  "correlationId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
  "messageType": [
    "urn:message:Contracts.IntegrationEvents:PaymentProcessedEventV1"
  ],
  "message": {
    "eventId": "9f7bec32-e63c-4c0b-ae0d-d6d57011287f",
    "occurredAtUtc": "2026-03-01T18:12:00Z",
    "paymentId": "6780b99e-d755-44d9-8bfc-6643a494181a",
    "orderId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
    "userId": "4d9de275-d990-42b9-9f5b-b52290ab3c1f",
    "status": "Approved",
    "reason": null,
    "schemaVersion": 1
  }
}
```

✅ **CatalogAPI** consome:
- Atualiza `Order` → `Confirmed`
- Adiciona `(userId, gameId)` na biblioteca (idempotente)

✅ **NotificationsAPI** consome e loga:
- `[EMAIL] Compra aprovada ✅ | OrderId=...`

---

## 4) (Opcional) Exemplo de rejeição (`Rejected`)

Se o Payments rejeitar, o evento seria:

```json
{
  "messageId": "1111ec32-e63c-4c0b-ae0d-d6d570112aaa",
  "correlationId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
  "messageType": [
    "urn:message:Contracts.IntegrationEvents:PaymentProcessedEventV1"
  ],
  "message": {
    "eventId": "1111ec32-e63c-4c0b-ae0d-d6d570112aaa",
    "occurredAtUtc": "2026-03-01T18:12:00Z",
    "paymentId": "2222b99e-d755-44d9-8bfc-6643a4941bbb",
    "orderId": "85461b76-b4ad-4b4c-a9ac-ecd609ee061e",
    "userId": "4d9de275-d990-42b9-9f5b-b52290ab3c1f",
    "status": "Rejected",
    "reason": "Insufficient funds",
    "schemaVersion": 1
  }
}
```