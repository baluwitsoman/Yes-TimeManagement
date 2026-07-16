# Deploying YES Time Management under IIS as a sub-application (`/YesTime`)

The app is designed to run as an IIS **sub-application** named `YesTime` under an
existing site (e.g. the current YES ERP site), reachable at `http://<server>/YesTime`.

## How the sub-path is handled
- Under IIS **in-process** hosting, the ASP.NET Core Module sets `Request.PathBase`
  to the virtual path (`/YesTime`) **automatically**. All links are generated with
  Razor tag helpers (`asp-page`, `~/…`), so they include the sub-path with no config.
- `Program.cs` also honours an optional `PathBase` config value — only needed for
  non-IIS reverse proxies (nginx, etc.). **Leave it empty for IIS.**
- Data-protection keys are persisted to `keys/` so auth cookies survive app-pool
  recycles (otherwise everyone is logged out on every recycle).

## Prerequisites (on the server)
1. **IIS** with the ASP.NET Core Module V2 — installed by the hosting bundle below.
2. **.NET 10 Hosting Bundle** (ASP.NET Core Runtime + ANCM):
   download "Hosting Bundle" from the .NET 10 downloads page, install, then
   `net stop was /y && net start w3svc` (or reboot) so IIS picks up the module.
3. Network access from the server to the **Oracle** instance. No Oracle client
   install is needed — the app uses the fully managed `Oracle.ManagedDataAccess.Core`.

## 1. Publish
From the repo root on a build machine (or the server):
```
dotnet publish src/YesTm.Web/YesTm.Web.csproj -c Release -o publish/YesTime
```
This produces a framework-dependent deployment including `web.config`
(AspNetCoreModuleV2, `hostingModel="inprocess"`). Copy `publish/YesTime` to the
server, e.g. `C:\inetpub\apps\YesTime`.

## 2. App pool
Create a dedicated pool so it doesn't share state with the parent site:
- **.NET CLR version = No Managed Code** (required — the runtime is loaded by ANCM).
- Pipeline: Integrated. Identity: `ApplicationPoolIdentity` (default).

```powershell
Import-Module WebAdministration
New-WebAppPool -Name "YesTimePool"
Set-ItemProperty IIS:\AppPools\YesTimePool -Name managedRuntimeVersion -Value ""
```

## 3. Create the sub-application
Under the parent site (e.g. `Default Web Site` or the YES ERP site):
```powershell
New-WebApplication -Site "Default Web Site" -Name "YesTime" `
  -PhysicalPath "C:\inetpub\apps\YesTime" -ApplicationPool "YesTimePool"
```
(or in IIS Manager: right-click the site → **Add Application** → Alias `YesTime`,
pool `YesTimePool`, physical path to the publish folder.)

## 4. Permissions
The app pool identity (`IIS AppPool\YesTimePool`) needs **Modify** on the folders
it writes to — the data-protection keys and logs:
```powershell
icacls "C:\inetpub\apps\YesTime\keys" /grant "IIS AppPool\YesTimePool:(OI)(CI)M" /T
icacls "C:\inetpub\apps\YesTime\logs" /grant "IIS AppPool\YesTimePool:(OI)(CI)M" /T
```
Create the `keys` and `logs` folders first if they don't exist.

## 5. Configuration
Edit `appsettings.json` (or set environment variables) in the publish folder:
- `ConnectionStrings:Oracle` — point at the production Oracle. Prefer an environment
  variable so the password isn't in the file:
  `ConnectionStrings__Oracle = User Id=…;Password=…;Data Source=host:1521/SERVICE;`
- `ASPNETCORE_ENVIRONMENT = Production` (default). The demo sign-in buttons only
  appear in Development, so they are **off** in production.
- `PathBase` — leave empty for IIS.

Set environment variables per-app-pool if desired:
```powershell
# Example: set the connection string as an app-pool env var
$env = @{ "ConnectionStrings__Oracle" = "User Id=YES_TM;Password=****;Data Source=host:1521/SERVICE;" }
```
(or keep it in appsettings.json).

## 6. Database
Run the schema + seed once against the production Oracle (idempotent):
```
dotnet run --project tools/DbMigrate -- "User Id=…;Password=…;Data Source=host:1521/SERVICE;" db/01_tm_schema.sql db/02_seed.sql db/03_alter_timesheet_equipment.sql
```
or run `db/01_tm_schema.sql`, `db/02_seed.sql`, `db/03_alter_timesheet_equipment.sql`
in SQL*Plus / SQLcl as the app schema owner. Then grant SITEADMIN to the intended
ERP user (see `db/02_seed.sql`).

## 7. Verify
Browse to `http://<server>/YesTime` — it should redirect to
`/YesTime/Authentication/Login`. Sign in with an ERP account that has a role.

## Troubleshooting
- **HTTP 500.30 / 500.31** — runtime failed to start; check the hosting bundle is
  installed and the pool is *No Managed Code*.
- Enable stdout logging temporarily: in `web.config` set `stdoutLogEnabled="true"`
  (ensure `logs/` is writable), reproduce, then read `logs/stdout_*.log`. Turn it
  back off afterwards.
- App logs are in `logs/yes-tm-*.log` (Serilog).
- **Logged out after a recycle** — the `keys/` folder isn't writable by the pool
  identity (see step 4).
- **HTTPS redirect loop / wrong port** — if the site is HTTP-only, either add an
  HTTPS binding or remove `app.UseHttpsRedirection()` for that environment.
