# BankApp

Banking app. ASP.NET Core server + WinUI 3 client.
Our team: authentication, dashboard, profile.
Other teams add their features to the same server.

## Architecture

Two programs running at the same time:

- **Server** (BankApp.Server) - ASP.NET Core Web API, runs in console, has all logic, only thing that talks to DB
- **Client** (BankApp.Client) - WinUI 3 desktop app, the UI, sends HTTP requests to server
- **BankApp.Models** - shared library, entities + enums + DTOs, referenced by both

User clicks Login -> client sends `POST /api/auth/login` with JSON -> server processes -> returns JSON -> client updates UI.

## Setup

### 1. Clone

```
git clone <repo-url>
```

### 2. Database

- Open SSMS
- Server Name: `localhost` (or `.\SQLEXPRESS` for SQL Express)
- Windows Authentication -> Connect
- New Query -> paste contents of `Database/CreateDatabase.sql` -> Execute
- Refresh Databases in left panel -> BankAppDb should appear with 11 tables

### 3. Connection string

Open `BankApp.Server/appsettings.json`, check the server name matches yours:
- Normal install: `Server=localhost`
- SQL Express: `Server=.\SQLEXPRESS`
- LocalDB: `Server=(localdb)\MSSQLLocalDB`

Don't commit this change. Add appsettings.json to .gitignore

### 4. API Secret / ClientID / SMTP code generations

For the SMTP Connection:
- Create/Open a Google Account
- Make sure 2FA is enabled for the google acc
- Create a new app password ex: "My-App-Pass"
- Open `BankApp.Server/appsettings.json`
- Modify the code from "SmtpPass": "My-App-Pass"

For external auth with Google:
- Open Google Cloud Platform
- Go to APIs and Services -> Credentials
- Create a new OAuthClient ID
- Open `OAuthSecrets.template.cs`
- Replace the strings with the generated codes.

### 5. Startup projects

Right-click Solution -> Properties -> Multiple startup projects -> set both Server and Client to "Start"

### 6. Build + run

- Ctrl+Shift+B -> should be 0 errors
- F5 -> server console opens, then client window
- Swagger at `http://localhost:5000/swagger` for testing endpoints

## Working on features

```
git checkout -b feature/your-name/what-you-do
# write code
git add .
git commit -m "what you did"
git push origin feature/your-name/what-you-do
# open Pull Request on GitHub
```

## API endpoints

### Auth
| Method | Path | Body | Returns |
|--------|------|------|---------|
| POST | /api/auth/login | LoginRequest | LoginResponse |
| POST | /api/auth/register | RegisterRequest | RegisterResponse |
| POST | /api/auth/verify-otp | VerifyOTPRequest | LoginResponse |
| POST | /api/auth/forgot-password | ForgotPasswordRequest | 200 OK |
| POST | /api/auth/reset-password | ResetPasswordRequest | 200 or 400 |

### Dashboard
| Method | Path | Body | Returns |
|--------|------|------|---------|
| GET | /api/dashboard/{userId} | — | DashboardResponse |

### Profile
| Method | Path | Body | Returns |
|--------|------|------|---------|
| GET | /api/profile/{userId} | — | User |
| PUT | /api/profile/{userId} | UpdateProfileRequest | 200 or 400 |
| PUT | /api/profile/{userId}/password | ChangePasswordRequest | 200 or 400 |
| GET | /api/profile/{userId}/notifications/preferences | — | List\<NotificationPreference\> |
| PUT | /api/profile/{userId}/notifications/preferences | List\<NotificationPreference\> | 200 or 400 |
