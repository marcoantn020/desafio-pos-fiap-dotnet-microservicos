adicionar referencias
```bash
    dotnet add reference /home/marco/POS/microservicos-POS/FIAPGame/contracts/Contracts/Contracts/Contracts.csproj
    dotnet add reference ../contracts/Contracts/Contracts.csproj
```

## Resultado Arquitetural Correto

- UsersAPI → publica evento do Contracts
- CatalogAPI → publica evento do Contracts
- PaymentsAPI → consome + publica evento do Contracts
- NotificationsAPI → consome evento do Contracts

Nenhum serviço depende do outro.

1 - criar pasta para nuget:
```bash
    mkdir -p ~/nuget-local
```

2 - gere o pacote:
```bash
dotnet clean -c Release
dotnet build -c Release
dotnet pack -c Release
```

```bash
  dotnet pack -c Release
```

```bash
ls -la bin/Release
```

```bash
  cp bin/Release/*.nupkg ~/nuget-local/
```

3 - Em cada repo de API, crie nuget.config na raiz:
```bash
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local" value="/seu/path/nuget-local" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

4 - Instale nas APIs(precisa estar na mesma pasta):
```bash
  dotnet add package SeuOrg.Contracts --version 1.0.0
```