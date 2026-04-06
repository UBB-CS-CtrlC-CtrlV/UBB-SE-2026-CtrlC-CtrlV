# BankApp.Server

ASP.NET Core Web API backend for BankApp.

## Running with Docker (recommended)

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### First-time setup

```bash
python scripts/server/setup-dev-env.py   # generates .env with auto-created secrets
docker compose up --build
```

The API is available at **http://localhost:5024** once the server prints `Application started`.

`--build` is only needed on first run or after code changes.

### Useful commands

```bash
docker compose up -d          # start in background
docker compose logs -f server # tail server logs
docker compose down           # stop (data volume preserved)
docker compose down -v        # stop and delete database volume
```

## Running without Docker

```bash
python scripts/server/setup-dev-secrets.py   # sets dotnet user-secrets
dotnet run --project src/BankApp.Server
```

Requires a local SQL Server instance. Connection string is in `appsettings.Development.json`.

## Configuration reference

| Key                                   | Docker / .env var             | Description                        |
|---------------------------------------|-------------------------------|------------------------------------|
| `ConnectionStrings:DefaultConnection` | `DB_SA_PASSWORD` (composed)   | ADO.NET connection string          |
| `Jwt:Secret`                          | `JWT_SECRET`                  | JWT signing key (min 32 chars)     |
| `Email:SmtpHost`                      | `EMAIL_SMTP_HOST`             | SMTP server hostname               |
| `Email:SmtpPort`                      | `EMAIL_SMTP_PORT`             | SMTP port (587 for STARTTLS)       |
| `Email:SmtpUser`                      | `EMAIL_SMTP_USER`             | SMTP username                      |
| `Email:SmtpPass`                      | `EMAIL_SMTP_PASS`             | SMTP password / app password       |
| `Email:FromAddress`                   | `EMAIL_FROM_ADDRESS`          | Sender address for outgoing emails |

## Production

Set the same environment variables on your host. Use `__` instead of `:` for nested keys (e.g. `Jwt__Secret`). Set `ASPNETCORE_ENVIRONMENT=Production` to disable Swagger and developer error pages.
