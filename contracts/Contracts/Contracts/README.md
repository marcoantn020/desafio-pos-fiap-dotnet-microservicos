# Contracts (FIAPGame)

Pacote com contratos de integração (Integration Events) compartilhados entre os microserviços. Este projeto publica um package NuGet para consumo pelas APIs, garantindo baixo acoplamento e versionamento centralizado.

## Visão Geral

- Objetivo: definir eventos e modelos de mensagem para comunicação assíncrona entre serviços.
- Benefícios:
    - Padronização de eventos entre equipes e repositórios.
    - Evolução com versionamento sem dependências diretas entre serviços.
    - Distribuição via NuGet local (e facilmente migrável para um feed privado).

## Resultado Arquitetural Correto

- UsersAPI → publica evento do Contracts
- CatalogAPI → publica evento do Contracts
- PaymentsAPI → consome + publica evento do Contracts
- NotificationsAPI → consome evento do Contracts

Nenhum serviço depende do outro.

## Fluxo de Referência Direta (opcional durante desenvolvimento)

adicionar referencias
```shell script
dotnet add reference /home/marco/POS/microservicos-POS/FIAPGame/contracts/Contracts/Contracts/Contracts.csproj
    dotnet add reference ../contracts/Contracts/Contracts.csproj
```


Use referências diretas apenas para desenvolvimento local rápido. Para integração entre repositórios, prefira o pacote NuGet.

## Empacotamento e Consumo via NuGet Local

1 - criar pasta para nuget:
```shell script
mkdir -p ~/nuget-local
```


2 - gere o pacote:
```shell script
dotnet clean -c Release
dotnet build -c Release
dotnet pack -c Release
```


```shell script
dotnet pack -c Release
```


```shell script
ls -la bin/Release
```


```shell script
cp bin/Release/*.nupkg ~/nuget-local/
```


3 - Em cada repo de API, crie nuget.config na raiz:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local" value="/seu/path/nuget-local" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```


4 - Instale nas APIs (precisa estar na mesma pasta):
```shell script
dotnet add package SeuOrg.Contracts --version 1.0.0
```


## Dicas e Boas Práticas

- Versionamento:
    - Utilize SemVer (ex.: 1.0.1, 1.1.0, 2.0.0).
    - Quebras de contrato exigem major version nova.
- Compatibilidade:
    - Evolua eventos de forma backward-compatible sempre que possível (novos campos opcionais).
- Publicação:
    - Mantenha CHANGELOG com alterações por versão.
- CI/CD (opcional):
    - Automatize pack/copy para o feed local ou para um repositório NuGet privado.
- Testes:
    - Valide serialização/deserialização (ex.: System.Text.Json).
    - Garanta contratos estáveis com testes de snapshot/esquema.

## Requisitos

- .NET SDK compatível com o target framework do projeto.
- Permissões de leitura no diretório do feed local (~/nuget-local).

## Solução de Problemas

- Pacote não encontrado:
    - Verifique nuget.config no repositório da API.
    - Rode:
```shell script
dotnet nuget locals all --clear
```

    e tente novamente.
- Versão incorreta:
    - Confirme a versão no .nupkg em bin/Release.
    - Atualize o comando de instalação com a versão correta.
- Múltiplos feeds:
    - Assegure a prioridade correta do feed local no nuget.config se houver conflitos de nome.