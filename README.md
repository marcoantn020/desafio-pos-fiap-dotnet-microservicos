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

---

## Executando com Docker Compose

### Pré-requisitos

- Docker e Docker Compose instalados

### Subir a stack completa

A partir da **raiz do repositório**:

```bash
# Primeira execução (faz build das imagens)
docker compose up -d

# Execuções seguintes (sem rebuild)
docker compose up -d

# Rebuild após mudanças no código
docker compose up -d --build

# Ver logs de todos os serviços
docker compose logs -f

# Derrubar tudo
docker compose down
```

### Portas expostas no host

| Serviço             | URL                              |
|---------------------|----------------------------------|
| UsersAPI            | http://localhost:5001/swagger    |
| CatalogAPI          | http://localhost:5002/swagger    |
| PaymentsAPI         | http://localhost:5003/health     |
| NotificationsAPI    | http://localhost:5004/health     |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| PostgreSQL          | localhost:5433                   |

> **Nota:** O PostgreSQL é exposto na porta `5433` para evitar conflito com instalações locais na porta padrão `5432`. A comunicação interna entre os containers usa a porta `5432`.

### Variáveis de ambiente

Cada serviço lê configuração via variáveis de ambiente que sobrescrevem o `appsettings.json`. A convenção ASP.NET Core usa `__` como separador de seção (ex: `RabbitMq__Host`). As variáveis estão definidas no `docker-compose.yml`.

---

## Deploy no Kubernetes (local)

### Pré-requisitos

Instalar **Kind** e **kubectl**:

```bash
# Kind
curl -Lo ./kind https://kind.sigs.k8s.io/dl/v0.23.0/kind-linux-amd64
chmod +x ./kind && sudo mv ./kind /usr/local/bin/kind

# kubectl
curl -LO "https://dl.k8s.io/release/$(curl -sL https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl && sudo mv kubectl /usr/local/bin/kubectl
```

### Build das imagens (necessário antes do deploy)

A partir da **raiz do repositório**:

```bash
docker build -f users-api/Dockerfile        -t fcg-users-api:latest .
docker build -f catalog-api/Dockerfile      -t fcg-catalog-api:latest .
docker build -f payments-api/Dockerfile     -t fcg-payments-api:latest .
docker build -f notications-api/Dockerfile  -t fcg-notifications-api:latest .
```

### Deploy completo (script automático)

```bash
./k8s-deploy.sh
```

O script cria o cluster Kind, carrega as imagens e aplica todos os manifests em ordem.

### Deploy manual passo a passo

```bash
# 1. Criar cluster
kind create cluster --name fcg-cluster

# 2. Carregar imagens no cluster
kind load docker-image fcg-users-api:latest        --name fcg-cluster
kind load docker-image fcg-catalog-api:latest      --name fcg-cluster
kind load docker-image fcg-payments-api:latest     --name fcg-cluster
kind load docker-image fcg-notifications-api:latest --name fcg-cluster

# 3. Aplicar manifests (infra primeiro, depois serviços)
kubectl apply -f infra/k8s/
kubectl apply -f users-api/k8s/
kubectl apply -f catalog-api/k8s/
kubectl apply -f payments-api/k8s/
kubectl apply -f notications-api/k8s/

# 4. Verificar pods
kubectl get pods

# 5. Verificar serviços
kubectl get services
```

### Estrutura dos manifests (`/k8s` em cada serviço)

| Arquivo          | Tipo       | Conteúdo                                        |
|------------------|------------|-------------------------------------------------|
| `configmap.yaml` | ConfigMap  | Host do RabbitMQ, filas, JWT issuer/audience    |
| `secret.yaml`    | Secret     | Connection string do DB, JWT key, senha RabbitMQ|
| `deployment.yaml`| Deployment | Pod com probes de readiness e liveness          |
| `service.yaml`   | Service    | NodePort para acesso externo                    |

Infra (`infra/k8s/`): Deployment + Service do PostgreSQL e RabbitMQ, PVC para o Postgres, ConfigMap com o script SQL de inicialização dos bancos.

### Acessando os serviços no Kind via port-forward

```bash
kubectl port-forward svc/users-api      5001:80 &
kubectl port-forward svc/catalog-api    5002:80 &
kubectl port-forward svc/payments-api   5003:80 &
kubectl port-forward svc/rabbitmq       15672:15672 &
```

### Portas NodePort

| Serviço             | NodePort |
|---------------------|----------|
| users-api           | 30001    |
| catalog-api         | 30002    |
| payments-api        | 30003    |
| notifications-api   | 30004    |
| RabbitMQ management | 30015    |

---

## Como Testar os Fluxos

Os exemplos abaixo usam `curl` + `jq`. Certifique-se de que a stack está rodando (`docker compose up -d`) antes de começar.

Para acompanhar os eventos em tempo real em um terminal separado:

```bash
docker compose logs -f payments-api notifications-api catalog-api
```

---

### Credenciais

| Usuário | Email | Senha | Role |
|---------|-------|-------|------|
| Admin (auto-criado em Development) | `admin@fcg.local` | `Admin123!` | Admin |
| Usuário comum | o que você cadastrar | o que você cadastrar | User |

> O usuário admin é criado automaticamente no startup do UsersAPI porque o `docker-compose.yml` define `ASPNETCORE_ENVIRONMENT: Development` para esse serviço.

---

### Fluxo 1 — Cadastro de Usuário → Notificação de boas-vindas

**1. Registrar um novo usuário**

```bash
curl -s -X POST http://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"jogador@fcg.com","password":"Senha123!","displayName":"João Jogador"}' \
  | jq .
```

Resposta esperada (`201 Created`):
```json
{
  "userId": "...",
  "email": "jogador@fcg.com",
  "displayName": "João Jogador",
  "accessToken": "eyJ..."
}
```

**2. Verificar o e-mail de boas-vindas nos logs da NotificationsAPI**

```bash
docker compose logs notifications-api | grep "EMAIL"
```

Saída esperada:
```
[EMAIL] Bem-vindo(a) João Jogador! Enviado para jogador@fcg.com.
```

---

### Fluxo 2 — Compra de Jogo → Pagamento → Biblioteca + Notificação

**3. Login como admin e salvar o token**

```bash
TOKEN=$(curl -s -X POST http://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fcg.local","password":"Admin123!"}' \
  | jq -r .accessToken)

echo $TOKEN   # deve exibir o JWT
```

**4. Criar um jogo no catálogo (requer role Admin)**

```bash
GAME_ID=$(curl -s -X POST http://localhost:5002/games \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Cyberpunk 2099","priceCents":19990,"currency":"BRL"}' \
  | jq -r .id)

echo $GAME_ID   # ID do jogo criado
```

**5. Login como usuário comum e salvar o token**

```bash
USER_TOKEN=$(curl -s -X POST http://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"jogador@fcg.com","password":"Senha123!"}' \
  | jq -r .accessToken)
```

**6. Fazer o pedido de compra**

```bash
curl -s -X POST http://localhost:5002/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"gameId\":\"$GAME_ID\"}" \
  | jq .
```

Resposta esperada (`202 Accepted`):
```json
{
  "id": "...",
  "status": "Pending",
  "eventId": "..."
}
```

A partir daqui o fluxo é assíncrono. Aguarde 2–5 segundos para os eventos propagarem.

**7. Verificar a notificação de pagamento**

```bash
docker compose logs notifications-api | grep "EMAIL"
```

Se aprovado:
```
[EMAIL] Compra aprovada ✅ | OrderId=... | UserId=...
```

Se rejeitado (20% de chance por configuração):
```
[EMAIL] Compra rejeitada ❌ | OrderId=... | Reason=...
```

**8. Verificar que o pedido foi atualizado**

```bash
curl -s http://localhost:5002/orders \
  -H "Authorization: Bearer $USER_TOKEN" \
  | jq .
```

O campo `status` deve ter mudado de `Pending` para `Confirmed` (aprovado) ou `Cancelled` (rejeitado).

**9. Verificar a biblioteca do usuário (apenas se aprovado)**

```bash
curl -s http://localhost:5002/library \
  -H "Authorization: Bearer $USER_TOKEN" \
  | jq .
```

O jogo deve aparecer na lista de jogos adquiridos.

---

### Alternativa: Swagger UI

Em vez de `curl`, use o Swagger para testar interativamente:

| Serviço | URL |
|---------|-----|
| UsersAPI | http://localhost:5001/swagger |
| CatalogAPI | http://localhost:5002/swagger |

No Swagger, clique em **Authorize** e cole o token no formato `Bearer eyJ...` para habilitar os endpoints protegidos.