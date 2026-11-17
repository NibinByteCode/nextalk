# NexTalk

NexTalk is a real-time chat application built with ASP.NET Core, SignalR,
Angular, SQL Server, and Firebase Cloud Firestore.

## Features

- JWT registration and login
- Private and group conversations
- Public chat rooms
- Persistent message history
- Real-time messaging with SignalR
- Provider-independent `IChatStore` persistence abstraction

## Run locally

Set a JWT secret of at least 32 bytes before starting the API:

```powershell
$env:Jwt__Secret = "replace-with-a-long-random-secret-at-least-32-bytes"
dotnet dev-certs https --trust
dotnet run --project backend\NexTalk.Api --launch-profile https
```

In another terminal:

```powershell
cd frontend\nextalk-web
npm start
```

The default frontend URL is `http://localhost:4200`.

The Angular development configuration uses `https://localhost:7000` for API
and SignalR traffic. Credentials are protected in transit by HTTPS; the API
also hashes passwords with ASP.NET Core's password hasher and never stores
plain-text passwords. Do not use the HTTP profile for authenticated traffic.

Set `Database__Provider` to `SqlServer` or `Firebase`. SQL Server uses
`ConnectionStrings__SqlServer`; Firebase uses `Firebase__ProjectId` and
secure server-side credentials.

Never commit JWT secrets or Firebase service-account files.
