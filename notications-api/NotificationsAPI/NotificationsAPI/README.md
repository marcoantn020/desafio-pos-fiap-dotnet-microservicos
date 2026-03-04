# NotificationsAPI

Serviço de notificações do FIAPGame, construído em ASP.NET Core (.NET 8). Processa eventos de integração via RabbitMQ/MassTransit e persiste mensagens de inbox no PostgreSQL para garantir idempotência. Exemplo: ao receber UserCreatedEventV1, registra consumo e simula envio de e-mail via logs.

## Tecnologias
- .NET 8, C# 12
- ASP.NET Core (Web API/MVC)
- MassTransit + RabbitMQ
- Entity Framework Core + PostgreSQL
- Logging via Microsoft.Extensions.Logging
- Razor/Swagger (se configurado)

## Pré-requisitos
- .NET SDK 8.0+
- Docker (opcional)
- PostgreSQL acessível conforme ConnectionStrings:Default
- RabbitMQ acessível conforme RabbitMq settings

## Configuração
Arquivo appsettings.json (valores padrão de desenvolvimento):
- Logging.LogLevel:
    - Default: Information
    - Microsoft.AspNetCore: Warning
    - Microsoft.EntityFrameworkCore.Database.Command: None
- ConnectionStrings.Default:
    - Host=localhost;Port=5432;Database=fcg_notifications_db;Username=notifications;Password=notificationspw
- RabbitMq:
    - Host: localhost
    - Username: guest
    - Password: guest
    - VirtualHost: /

Ambiente:
- Configure ASPNETCORE_ENVIRONMENT (Development/Production).
- Em produção, use variáveis de ambiente/KeyVault para segredos.

## Como executar localmente
1) Restaurar dependências:
- dotnet restore

2) Aplicar migrations (se o projeto as possuir):
- dotnet ef database update

3) Executar:
- dotnet run --project NotificationsAPI

4) Endpoints úteis:
- Swagger: /swagger (se habilitado)
- Health: /health (se configurado)

Certifique-se de que:
- PostgreSQL esteja rodando e com o banco/usuário configurados conforme a string de conexão.
- RabbitMQ esteja rodando e acessível com as credenciais em appsettings.json.

## Consumidores de eventos
- UserCreatedConsumer
    - Entrada: Contracts.IntegrationEvents.UserCreatedEventV1
    - Comportamento:
        - Verifica idempotência via tabela InboxMessages (Id=EventId, Consumer=nome do consumidor).
        - Persiste consumo e registra em log um “envio” de e-mail de boas-vindas.
    - Observação: o envio de e-mail está simulado via log, podendo ser substituído por um provedor real.

## Padrões aplicados
- Idempotência de mensagens (Inbox pattern)
- Injeção de dependência
- Configurações via Options + appsettings.json
- Logging estruturado

## Estrutura recomendada de pastas (ajustar ao seu repositório)
- NotificationsAPI/
    - Consumers/
    - Infrastructure/
        - Persistence/ (DbContext, Migrations)
    - Program.cs
    - appsettings*.json

#### Por Marco Antonio