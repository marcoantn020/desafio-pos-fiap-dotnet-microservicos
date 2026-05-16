FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-contracts
WORKDIR /contracts
COPY contracts/Contracts/Contracts/ .
RUN dotnet pack -c Release -o /nuget-local

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY --from=build-contracts /nuget-local /nuget-local
COPY notications-api/NotificationsAPI/NotificationsAPI/ .
RUN printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <packageSources>\n    <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>\n    <add key="contracts-local" value="/nuget-local"/>\n  </packageSources>\n</configuration>' > nuget.config
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationsAPI.dll"]
