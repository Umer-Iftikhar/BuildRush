# BuildRush

A web-based auction platform built with ASP.NET Core, featuring real-time bidding notifications powered by SignalR. Users receive instant alerts across all pages when they are outbid or when auction activity occurs — no page refresh needed.

---

## Features

- Real-time notifications via SignalR — fires globally across all pages
- Notification bell in the navbar that updates live wherever the user is
- Live bid updates on auction pages
- JWT authentication with refresh token rotation
- Per-user notification targeting — users only receive their own alerts
- Automatic reconnection if the connection drops

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core |
| Real-Time | SignalR |
| Frontend | JavaScript (SignalR JS Client) |
| Database | Microsoft SQL Server (MSSQL) |
| Data Access | Dapper + Stored Procedures |
| Authentication | JWT / Cookie auth with refresh token rotation |
| Config | `.env` file |

---

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- Microsoft SQL Server
- A `.env` file configured with the required credentials (see below)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Umer-Iftikhar/BuildRush.git
cd BuildRush
```

### 2. Set up your `.env` file

Create a `.env` file in the root of the project. This is where all credentials and configuration live — do not commit this file.

```env
DB_CONNECTION_STRING=Server=your-server;Database=BuildRush;User Id=your-user;Password=your-password;
JWT_SECRET=your-jwt-secret-key
JWT_ISSUER=your-issuer
JWT_AUDIENCE=your-audience
JWT_EXPIRY_MINUTES=60
REFRESH_TOKEN_EXPIRY_DAYS=7
```

### 3. Set up the database

All database objects are stored in the `Database/` folder.

Run the scripts in this order:

```text
  Database/
  ├── Scripts/ — run these first (tables, seed data)
  └── StoredProcedures/ — run these after
  ```
  
Execute them against your MSSQL instance using SSMS or `sqlcmd`.

### 4. Run the application

```bash
cd BuildRush
dotnet run
```

The app will be available at `https://localhost:PORT` — check `launchSettings.json` for the port.

---

## Project Structure

```text
BuildRush/
│
├── BuildRush/                     # Main ASP.NET Core project
│ ├── Constants/                   # App-wide constant values
│ ├── Controllers/                 # API and MVC controllers
│ ├── CustomAttributes/            # Custom action filter attributes
│ ├── Data/                        # Dapper data access — calls stored procedures
│ ├── DTOs/
│ │ ├── Request/                   # Incoming request models
│ │ ├── Response/                  # Outgoing response models
│ │ └── Internal/                  # DTOs used between layers internally
│ ├── Enum/                        # Application enumerations
│ ├── Hub/                         # SignalR Hub — NotificationHub lives here
│ ├── Middleware/                  # Custom middleware (auth, error handling, etc.)
│ ├── Services/
│ │ ├── Interfaces/                # Service contracts
│ │ └── Implementations/           # Service logic
│ ├── Settings/                    # Strongly typed config classes (JWT settings etc.)
│ ├── ViewModels/                  # View-bound models
│ ├── Views/                       # Razor views
│ ├── wwwroot/                     # Static files — JS, CSS, images
│ ├── appsettings.json             # Non-sensitive app configuration
│ ├── lbman.json                  
│ └── Program.cs                   # App entry point and service registration
│
└── Database/
├── Scripts/                       # Table creation and seed scripts
└── StoredProcedures/              # All stored procedures used by Dapper
```

---

## Authentication

Authentication uses **JWT tokens with cookie transport and refresh token rotation** — there is no dependency on ASP.NET Identity.

- On login, the server issues an access token and a refresh token
- The access token is short-lived; the refresh token is rotated on each use
- Tokens are stored and validated via cookie
- On each SignalR connection, the server reads the user identity directly from the token — the client never sends a user ID manually

Refresh token rotation means each time a refresh token is used to get a new access token, the old refresh token is invalidated and a new one is issued. This limits the window of exposure if a token is ever leaked.

---

## How the Notification System Works

Notifications are global — they follow the user across every page, not just the auction page.

### Flow

1. On any page load, the SignalR client in `wwwroot/js/site.js` opens a connection to `/notificationHub`
2. The server reads the authenticated user's identity from the JWT — no user ID is passed from the client
3. The server adds the connection to a group named `user-{userId}`
4. When an auction event fires (e.g. user is outbid), the backend sends to that group via `IHubContext` from the relevant service
5. The client receives it and updates the notification bell in the navbar — on whatever page the user is currently on

### Hub — OnConnected

```csharp
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
}
```

### Sending a notification from a service

```csharp
await _hubContext.Clients
    .Group($"user-{userId}")
    .SendAsync("ReceiveNotification", notification);
```

### Client — site.js (runs on every page)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", function(notification) {
    showNotificationBadge(notification);
});

connection.start().catch(err => console.error(err));
```

---

## Data Access

All database interaction goes through **Dapper** calling **stored procedures** — there is no Entity Framework or LINQ-to-SQL.

Stored procedures live in `Database/StoredProcedures/` and are the single source of truth for all queries. The `Data/` layer in the application maps results to DTOs and passes them up to services.

---

## Known Limitations

- **Offline delivery:** Notifications are not queued. If a user is disconnected when a notification is sent, they will not receive it. A database-backed notification log with fetch-on-reconnect would address this.
- **UI:** Notification UI is functional but styling is minimal — frontend polish is post-deadline.
- **Scaling:** Connection state is held in memory. Multi-instance deployments would need a SignalR backplane (Redis or Azure SignalR Service).

---

## Production Checklist

- Set `ASPNETCORE_ENVIRONMENT=Production`
- Ensure `.env` is not committed and is excluded in `.gitignore`
- Configure CORS — SignalR requires `AllowCredentials()`
- If deploying behind a reverse proxy (nginx / IIS), enable WebSocket support
- For multi-server deployments, add Redis backplane:

```csharp
builder.Services.AddSignalR().AddStackExchangeRedis("your-redis-connection-string");
```

---
