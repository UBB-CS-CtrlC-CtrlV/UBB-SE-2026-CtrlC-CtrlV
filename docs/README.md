# BankApp — Project Documentation

## Overview

BankApp is a desktop banking application built with an **ASP.NET Core** REST API backend and a **WinUI 3** desktop client. Multiple teams contribute features to the same shared server and database.

**This team's scope (CtrlC CtrlV):** Authentication, Dashboard, Profile.

---

## Architecture

Three layers communicating over HTTP:

| Layer | Technology | Responsibility |
|---|---|---|
| Presentation | WinUI 3 (C#) | Desktop UI — Views, ViewModels, navigation |
| Business Logic | ASP.NET Core Web API (C#) | Controllers, Services, Repositories, Validators |
| Data | SQL Server | Persistence for all domains |

**Runtime model:** Server and Client run as two separate processes. The client sends HTTP requests; only the server talks to the database.

See [`diagrams/architecture.png`](diagrams/architecture.png) for the full layer diagram.

---

## Features

### This team (CtrlC CtrlV)

| Feature | Description |
|---|---|
| Authentication | Email/password login and registration, 2FA (email & SMS), forgot/reset password, Google OAuth |
| Dashboard | Account summary, card carousel, recent transactions, notification badge |
| Profile | View/edit personal info, change password, 2FA toggle, OAuth account links, notification preferences |

### Full application (all teams)

| Area | Features |
|---|---|
| Auth & Security | Login, Register, 2FA, Forgot Password, Google OAuth |
| Dashboard | Account overview, card management, charts/stats |
| Transfers | Bank transfers between accounts |
| Bill Payments | Pay utility and service bills |
| Transaction History | View and filter past transactions |
| Currency Exchange | Live FX rates |
| Chat | In-app chat sessions and messages |
| Savings / Loans | Savings accounts, loan management |
| Portfolio / Investments | Stock price tracking |
| Notifications | Notification dispatch and audit logs |

---

## External Integrations

| Service | Purpose |
|---|---|
| Google OAuth | Social login and registration |
| Other OAuth providers | Additional social login options |
| SMTP / SendGrid | Email delivery (2FA codes, password reset) |
| SMS Gateway | SMS-based 2FA codes |
| Exchange Rate API | Live FX rates for currency exchange |
| Market Data API | Stock prices for portfolio view |

---

## Database

SQL Server with 11 tables covering:

- User / Auth / Session
- Account / Card
- Transfer / Bill Payment
- Transaction / Category
- Savings / Loan
- Portfolio / Investment
- Chat Session / Message
- Audit Log / Notification

See [`diagrams/database.png`](diagrams/database.png) for the full schema diagram.

---

## Diagrams

| File | Description |
|---|---|
| [`diagrams/architecture.png`](diagrams/architecture.png) | High-level 3-layer architecture |
| [`diagrams/architecture.xml`](diagrams/architecture.xml) | Editable source (draw.io) |
| [`diagrams/database.png`](diagrams/database.png) | Full database schema |
| [`diagrams/database.xml`](diagrams/database.xml) | Editable source (draw.io) |
| [`diagrams/class-diagram.png`](diagrams/class-diagram.png) | Class diagram |
| [`diagrams/class-diagram.xml`](diagrams/class-diagram.xml) | Editable source (draw.io) |
| [`diagrams/use-case.jpg`](diagrams/use-case.jpg) | Use case diagram |
| [`diagrams/use-case.mdj`](diagrams/use-case.mdj) | Editable source (StarUML) |
| [`requirements.pdf`](requirements.pdf) | Full requirements specification |
