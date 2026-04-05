# BankApp.Client

WinUI 3 desktop client for BankApp.

## Prerequisites

The client talks to `BankApp.Server` over HTTP. The server must be running locally
before you launch the client. See `src/BankApp.Server/README.md` for server setup.

## Local development setup

Sensitive configuration (OAuth credentials) is not stored in source control. Run the
setup script once from the repo root after cloning:

```bash
python scripts/client/setup-dev-config.py
```

The script creates `src/BankApp.Client/appsettings.Local.json` with your OAuth
credentials. This file is gitignored and will never be committed.

OAuth credentials must be obtained from the Google Cloud Console:
> console.cloud.google.com > APIs & Services > Credentials > OAuth 2.0 Client IDs

Re-running the script is safe — the file is fully overwritten each time.

### Configuration files

| File | Committed | Purpose |
|---|---|---|
| `appsettings.json` | Yes | Safe defaults and placeholders |
| `appsettings.Local.json` | No | Local overrides — secrets go here |

Config is loaded in this order, each layer overriding the previous:
`appsettings.json` → `appsettings.Local.json` → Environment variables

For CI or non-Windows environments, set values via environment variables using `__`
as the key separator (e.g. `OAuth__Google__ClientId`).

### Configuration reference

| Key | Default | Secret |
|---|---|---|
| `ApiBaseUrl` | `http://localhost:5024` | No |
| `OAuth:Google:Authority` | `https://accounts.google.com` | No |
| `OAuth:Google:RedirectUri` | `http://127.0.0.1:7890/` | No |
| `OAuth:Google:ClientId` | — | Yes — set via script |
| `OAuth:Google:ClientSecret` | — | Yes — set via script |

## Architecture

The client follows the **MVVM** (Model-View-ViewModel) pattern:

- **Views** are XAML pages with no business logic. They bind to their ViewModel and
  forward user actions to it.
- **ViewModels** coordinate API calls and hold UI state as `ObservableState<TState>`
  (a thin wrapper over state enums). Views observe state changes and update the UI.
- **ApiClient** is the single HTTP boundary. All network calls go through it.
  It is registered as a singleton to avoid socket exhaustion.
- **AppNavigationService** wraps the WinUI `Frame` and is the only place allowed to
  trigger page navigation.

Dependency injection is set up manually in `App.xaml.cs` using
`Microsoft.Extensions.DependencyInjection` — there is no generic host. The container
is available via `App.Services` but should only be accessed at the composition root
boundary (`OnLaunched`). All other classes receive dependencies via constructor injection.

## Running the app

1. Start `BankApp.Server` first (see its README).
2. Open the solution in Visual Studio 2022.
3. Set `BankApp.Client` as the startup project.
4. Select the `x64` platform and press F5.
