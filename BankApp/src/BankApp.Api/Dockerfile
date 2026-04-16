FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Solution-level files needed for StyleCop during compilation.
COPY Directory.Build.props .
COPY SE.ruleset .
COPY stylecop.json .

# Restore dependencies in a separate layer for cache efficiency.
COPY src/BankApp.Domain/BankApp.Domain.csproj          src/BankApp.Domain/
COPY src/BankApp.Application/BankApp.Application.csproj src/BankApp.Application/
COPY src/BankApp.Infrastructure/BankApp.Infrastructure.csproj src/BankApp.Infrastructure/
COPY src/BankApp.Api/BankApp.Api.csproj                src/BankApp.Api/
RUN dotnet restore src/BankApp.Api/BankApp.Api.csproj

COPY src/BankApp.Domain/       src/BankApp.Domain/
COPY src/BankApp.Application/  src/BankApp.Application/
COPY src/BankApp.Infrastructure/ src/BankApp.Infrastructure/
COPY src/BankApp.Api/          src/BankApp.Api/
RUN dotnet publish src/BankApp.Api/BankApp.Api.csproj \
    --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BankApp.Api.dll"]
