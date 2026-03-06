# Contracts (FIAPGame)

Pacote com contratos de integração (Integration Events) compartilhados entre os microserviços. Publicado no NuGet para consumo direto pelas APIs, garantindo baixo acoplamento e versionamento centralizado.

## Visão Geral

- Objetivo: definir eventos e modelos de mensagem para comunicação assíncrona entre serviços.
- Benefícios:
  - Padronização de eventos entre equipes e repositórios.
  - Evolução com versionamento sem dependências diretas entre serviços.
  - Distribuição via NuGet público.

## Arquitetura de Uso

- UsersAPI → publica evento do Contracts
- CatalogAPI → publica evento do Contracts
- PaymentsAPI → consome + publica evento do Contracts
- NotificationsAPI → consome evento do Contracts

Nenhum serviço depende diretamente do outro.

## Instalação via NuGet

O pacote está publicado no NuGet.org. Para instalar:
```bash
dotnet add package FIAPCloudGames2026.Contracts --version 1.0.0
```


## Requisitos

- .NET SDK compatível com o target framework do projeto (net8.0).
