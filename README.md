# iot-middleware

MQTT sensor middleware for my home. Run with Aspire: an ASP.NET Core API, a sensor simulator, an Angular dashboard, Mosquitto and TimescaleDB.

## Prerequisites

- .NET 10 SDK
- Node LTS
- Docker, running

## Setup

```powershell
dotnet tool restore
cd src/web
npm ci
cd ../..
```

Set the admin login once. The script prints the password's hash, and `user-secrets` stores the username and hash:

```powershell
dotnet run tools/hash-password.cs -- <password>
dotnet user-secrets set "Parameters:admin-username" "<username>" --project src/HomeClimate.AppHost
dotnet user-secrets set "Parameters:admin-password-hash" "<hash>" --project src/HomeClimate.AppHost
```

## Run

```powershell
dotnet aspire run
```

Open the dashboard link it prints, then open `web`. Log in with your username and password. The page shows "API ok, DB ok, broker ok".

## Build

```powershell
dotnet build HomeClimate.slnx
cd src/web
npx ng build
```

## Lint

```powershell
dotnet format HomeClimate.slnx --verify-no-changes
cd src/web
npx ng lint
npx prettier --check src
```

## Test

```powershell
dotnet test HomeClimate.slnx
cd src/web
npx ng test --watch=false
```

`dotnet test` starts the real AppHost, so Docker must be running.

## Layout

| Path | What |
|---|---|
| `src/HomeClimate.AppHost` | Aspire AppHost. Declares every resource. |
| `src/HomeClimate.AppHost/mosquitto` | Broker config, ACL and the entrypoint that hashes passwords at start. |
| `src/HomeClimate.ServiceDefaults` | Shared telemetry, health checks and service discovery. |
| `src/HomeClimate.Api` | API: admin cookie login and health endpoint, later MQTT ingest, alerts and SSE. |
| `src/HomeClimate.Simulator` | Stands in for real sensors. |
| `src/web` | Angular dashboard. |
| `tests/HomeClimate.Tests` | Aspire end-to-end tests. |
| `tools` | `hash-password.cs`, a one-file script that hashes the admin password. |

MQTT passwords are Aspire secret parameters. They are generated on first run and kept in the AppHost's user secrets. The admin username and password hash are secret parameters too, but you set them yourself, as shown in Setup.
