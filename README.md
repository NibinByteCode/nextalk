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
dotnet run --project backend\NexTalk.Api
```

In another terminal:

```powershell
cd frontend\nextalk-web
npm start
```

The default frontend URL is `http://localhost:4200`.

Set `Database__Provider` to `SqlServer` or `Firebase`. SQL Server uses
`ConnectionStrings__SqlServer`; Firebase uses `Firebase__ProjectId` and
secure server-side credentials.

Never commit JWT secrets or Firebase service-account files.
