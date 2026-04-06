FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Solution-level files needed for StyleCop during compilation.
COPY Directory.Build.props .
COPY SE.ruleset .
COPY stylecop.json .

# Restore dependencies in a separate layer for cache efficiency.
COPY src/BankApp.Contracts/BankApp.Contracts.csproj src/BankApp.Contracts/
COPY src/BankApp.Server/BankApp.Server.csproj      src/BankApp.Server/
RUN dotnet restore src/BankApp.Server/BankApp.Server.csproj

COPY src/BankApp.Contracts/ src/BankApp.Contracts/
COPY src/BankApp.Server/    src/BankApp.Server/
RUN dotnet publish src/BankApp.Server/BankApp.Server.csproj \
    --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BankApp.Server.dll"]
