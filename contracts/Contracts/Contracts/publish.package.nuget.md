# 📦 Como publicar um pacote NuGet

---

## 1️⃣ Preparar o projeto

No projeto que você quer transformar em biblioteca (geralmente Class Library), edite o `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>

    <PackageId>SeuPacote.Exemplo</PackageId>
    <Version>1.0.0</Version>
    <Authors>Marco</Authors>
    <Company>SuaEmpresa</Company>
    <Description>Biblioteca para exemplo</Description>
    <PackageTags>util;helpers</PackageTags>
    <RepositoryUrl>https://github.com/seuusuario/seuprojeto</RepositoryUrl>

    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>

</Project>
```

### Campos importantes

| Campo | O que faz |
|---|---|
| `PackageId` | Nome do pacote no NuGet |
| `Version` | Versão |
| `Authors` | Autor |
| `Description` | Descrição |
| `GeneratePackageOnBuild` | Gera o pacote automaticamente no build |

---

## 2️⃣ Gerar o pacote NuGet

No Rider você pode gerar de duas formas:

### Método 1 — Build

```
Build → Build Solution
```

Se `GeneratePackageOnBuild=true`, o `.nupkg` será criado em:

```
bin/Release/SeuPacote.Exemplo.1.0.0.nupkg
```

### Método 2 — CLI (recomendado)

Abra o terminal no Rider e execute:

```bash
dotnet pack -c Release
```

---

## 3️⃣ Criar conta no NuGet

Crie uma conta em 👉 [https://www.nuget.org](https://www.nuget.org)

Depois vá em:

```
Account → API Keys
```

Crie uma **API Key** — você vai usá-la para publicar.

---

## 4️⃣ Publicar o pacote

No terminal do Rider/VsCode execute:

```bash
dotnet nuget push bin/Release/SeuPacote.Exemplo.1.0.0.nupkg \
  --api-key SUA_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

Se tudo estiver correto:

```
Your package was pushed.
```

> Após alguns minutos o pacote aparece no site do NuGet.