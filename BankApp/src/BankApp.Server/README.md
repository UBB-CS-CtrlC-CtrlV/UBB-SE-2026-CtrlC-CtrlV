# BankApp.Server

ASP.NET Core Web API backend for BankApp.

## Running with Docker (recommended)

Docker Compose is the primary development setup. It starts SQL Server and the
API server together with a single command, requiring no local .NET SDK or SQL
Server installation.

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### First-time setup

```bash
# 1. Create your local secrets file from the template
cp .env.example .env

# 2. Fill in the values in .env (see comments inside the file)
#    — DB_SA_PASSWORD must meet SQL Server complexity rules
#    — JWT_SECRET: generate with: python scripts/server/setup-dev-secrets.py
#    — EMAIL_*: use a Gmail App Password (see note below)

# 3. Start everything
docker compose up --build
```

The API is available at **http://localhost:5024** once the server container
prints `Application started`.

The `--build` flag is only needed on the first run or after code changes.
Use `docker compose up` for subsequent starts.

> **Gmail App Password:** use an App Password, not your account password.
> Generate one at Google Account → Security → 2-Step Verification → App passwords.

### Useful commands

```bash
docker compose up -d          # start in background
docker compose logs -f server # tail server logs
docker compose down           # stop containers (data volume is preserved)
docker compose down -v        # stop and delete the database volume
```

---

## Running without Docker (alternative)

If you prefer to run the server directly with the .NET SDK, configure secrets
via the setup script:

```bash
python scripts/server/setup-dev-secrets.py
```

Requires Python 3.6+ and the .NET SDK. The script generates a random JWT
secret and prompts for SMTP credentials. Secrets are stored at
`%APPDATA%\Microsoft\UserSecrets\6321c0d0-8c18-47df-b537-61e283ac2339\secrets.json`
and never committed to the repository.

The database defaults to `localhost` with Windows authentication
(`appsettings.Development.json`). A local SQL Server instance must be running.

---

## Configuration reference

All sensitive values are supplied at runtime via environment variables (Docker)
or .NET User Secrets (bare-metal). The `appsettings.json` placeholders document
the expected keys.

| Key                                   | Docker / .env var             | Description                        |
|---------------------------------------|-------------------------------|------------------------------------|
| `ConnectionStrings:DefaultConnection` | `DB_SA_PASSWORD` (composed)   | ADO.NET connection string          |
| `Jwt:Secret`                          | `JWT_SECRET`                  | JWT signing key (min 32 chars)     |
| `Email:SmtpHost`                      | `EMAIL_SMTP_HOST`             | SMTP server hostname               |
| `Email:SmtpPort`                      | `EMAIL_SMTP_PORT`             | SMTP port (587 for STARTTLS)       |
| `Email:SmtpUser`                      | `EMAIL_SMTP_USER`             | SMTP username                      |
| `Email:SmtpPass`                      | `EMAIL_SMTP_PASS`             | SMTP password / app password       |
| `Email:FromAddress`                   | `EMAIL_FROM_ADDRESS`          | Sender address for outgoing emails |

## Production deployment

Set the same environment variables on your host or in your pipeline. Use `__`
(double underscore) instead of `:` when the platform does not support nested
keys natively (e.g. `Jwt__Secret`).

Set `ASPNETCORE_ENVIRONMENT=Production` to disable developer error pages and
Swagger UI.
