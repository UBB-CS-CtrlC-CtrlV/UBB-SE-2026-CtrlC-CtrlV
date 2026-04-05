# BankApp.Server

ASP.NET Core Web API backend for BankApp.

## Local development setup

Sensitive configuration values are not stored in source control. They must be set
via .NET User Secrets before running the server locally.

Run the setup script from the solution root:

```bash
python scripts/server/setup-dev-secrets.py
```

Requires Python 3.6+ and the .NET SDK. No third-party packages needed.
The script prompts for the database connection string (defaults to localhost with
Windows authentication), generates a cryptographically random JWT secret
automatically, and prompts you for the SMTP credentials (which must be real Gmail
values).

To verify your secrets are set afterwards:

```bash
dotnet user-secrets list --project src/BankApp.Server
```

Values are stored at `%APPDATA%\Microsoft\UserSecrets\6321c0d0-8c18-47df-b537-61e283ac2339\secrets.json`
on your machine and are never committed to the repository.

> **Gmail App Password:** use a Gmail App Password, not your account password.
> Generate one at Google Account > Security > 2-Step Verification > App passwords.

### Configuration files

| File | Committed | Purpose |
|---|---|---|
| `appsettings.json` | Yes | Non-secret defaults (log levels, SMTP host/port) |
| `appsettings.Development.json` | Yes | Development log-level overrides |
| `appsettings.Production.json` | Yes | Production log-level overrides |

Secrets and connection strings are **never** in source-controlled files. In
development they come from User Secrets; in CI/production they come from
environment variables.

## CI / Production configuration

Set the following environment variables in your pipeline or host. Use `__` (double
underscore) as the separator instead of `:`.

| Environment variable                  | Description                        |
|---------------------------------------|------------------------------------|
| `ConnectionStrings__DefaultConnection`| Full ADO.NET connection string     |
| `Jwt__Secret`                         | JWT signing key (min 32 chars)     |
| `Email__SmtpUser`                     | SMTP username                      |
| `Email__SmtpPass`                     | SMTP password / app password       |
| `Email__FromAddress`                  | Sender address for outgoing emails |

`Email__SmtpHost` and `Email__SmtpPort` default to `smtp.gmail.com:587` and only
need to be overridden if using a different mail provider.
