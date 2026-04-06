# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level files first so StyleCop and the ruleset are available
# during compilation. These rarely change so they land in an early cache layer.
COPY Directory.Build.props .
COPY SE.ruleset .
COPY stylecop.json .

# Copy project manifests in isolation to maximise layer caching: the restore
# layer is only invalidated when a .csproj changes, not when source changes.
COPY src/BankApp.Core/BankApp.Core.csproj                 src/BankApp.Core/
COPY src/BankApp.Infrastructure/BankApp.Infrastructure.csproj src/BankApp.Infrastructure/
COPY src/BankApp.Server/BankApp.Server.csproj              src/BankApp.Server/

RUN dotnet restore src/BankApp.Server/BankApp.Server.csproj

# Copy full source and publish.
COPY src/BankApp.Core/         src/BankApp.Core/
COPY src/BankApp.Infrastructure/ src/BankApp.Infrastructure/
COPY src/BankApp.Server/       src/BankApp.Server/

RUN dotnet publish src/BankApp.Server/BankApp.Server.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The aspnet base image sets ASPNETCORE_HTTP_PORTS=8080 by default.
# docker-compose maps the host port to this.
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankApp.Server.dll"]
