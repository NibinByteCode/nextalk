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

The Angular client uses the browser's hostname for API and SignalR traffic,
so the same build works from another device on the LAN. The API binds to all
network interfaces on HTTPS port `7000`. Credentials are protected in transit
by HTTPS; the API also hashes passwords with ASP.NET Core's password hasher
and never stores plain-text passwords.

### LAN development

Find the host computer's LAN IPv4 address, then configure the frontend origin
before starting the API:

```powershell
$lanIp = "10.0.0.212"
$env:AllowedOrigins__1 = "http://$lanIp`:4200"
$env:Jwt__Secret = "replace-with-a-long-random-secret-at-least-32-bytes"
dotnet run --project backend\NexTalk.Api --launch-profile https
```

Start Angular so other devices can connect:

```powershell
cd frontend\nextalk-web
npm start -- --host 0.0.0.0
```

Open `http://10.0.0.212:4200` from another device. Replace the example IP
with the host's actual LAN address. Allow inbound TCP ports `4200` and `7000`
through the host firewall if required.

The default .NET development certificate is trusted for `localhost`, not a
LAN IP. For encrypted LAN traffic, create a development certificate whose SAN
contains the host IP, configure it with
`ASPNETCORE_Kestrel__Certificates__Default__Path` and
`ASPNETCORE_Kestrel__Certificates__Default__Password`, and trust that
certificate on each client device. Do not use the HTTP profile for
authenticated traffic.

### HTTP LAN fallback for mobile testing

If installing a LAN certificate on the phone is not practical, use this only
on a trusted private Wi-Fi network. This mode does not encrypt credentials or
messages:

```powershell
$lanIp = "10.0.0.212"
$env:AllowedOrigins__1 = "http://$lanIp`:4200"
$env:Jwt__Secret = "replace-with-a-long-random-secret-at-least-32-bytes"
dotnet run --project backend\NexTalk.Api --launch-profile http
```

Then start Angular and open `http://10.0.0.212:4200` on the phone:

```powershell
cd frontend\nextalk-web
npm start -- --host 0.0.0.0
```

The frontend automatically uses the HTTP API when loaded over HTTP. Do not
use this mode outside a trusted development network.

Set `Database__Provider` to `SqlServer` or `Firebase`. SQL Server uses
`ConnectionStrings__SqlServer`; Firebase uses `Firebase__ProjectId` and
secure server-side credentials.

Never commit JWT secrets or Firebase service-account files.
