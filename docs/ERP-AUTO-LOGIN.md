# Single Sign-On: ERP → Time Management (auto-login)

This describes the **ERP-side** change that lets a user sign in once at the legacy YES ERP
login and be carried straight into the Time Management app (`/YesTime`) already authenticated.
The receiving half (token redemption, role auto-provisioning, cookie sign-in) already lives in
the TM app at `Features/Authentication/AutoLogin`.

> The ERP is a **separate solution** (ASP.NET WebForms / .NET Framework). The snippet below is
> a drop-in for its `Login.aspx.cs`; it can't be built or tested from the TM repo. The file at
> `old-erp-login-reference/` in this repo is a read-only reference copy of that page.

## How it works

Both apps share the same Oracle DB but can't share auth cookies. The handoff is a **one-time,
short-lived token row** in `TM_AUTO_LOGIN_TOKEN` (created by `db/05_auto_login_token.sql`):

1. User signs in at the ERP and picks one of the three Time-Management roles.
2. ERP inserts a random token (user id + ERP role name, `SYSDATE + 2 min` expiry) and
   redirects to `{TM_BASE_URL}/Authentication/AutoLogin?token=<token>`.
3. TM atomically redeems the token (unused + unexpired), maps the ERP role to a TM role
   (`TimeManagement→USER`, `TM-Admin→ADMIN`, `TM-SiteAdmin→SITEADMIN`), creates a
   `TM_USER_ROLE` row **only if the user has none**, signs the user in, and lands on the
   dashboard.

Roles that are **not** Time-Management roles keep the existing ERP path
(`CreateSession(...) → ~/Home.aspx`) unchanged.

## Prerequisites (ERP data / config)

- The three roles must exist in `AMM_ROLE_DETAILS.ROLE_NAME` exactly as
  `TimeManagement`, `TM-Admin`, `TM-SiteAdmin`, and be linked to the relevant users via
  `AMM_USER_ROLE_LNK_DETAILS` so they appear in the login role dropdown (`ddlRole`).
- Add the TM base URL to the ERP `web.config`:
  ```xml
  <appSettings>
    <add key="TM_BASE_URL" value="https://your-erp-host/YesTime" />
  </appSettings>
  ```

## Code — drop into `Login.aspx.cs`

The ERP login already resolves the selected role id (`ddlRole.SelectedValue`) and the user name.
Add this helper and call it right **before** the normal `CreateSession(...) → ~/Home.aspx` step,
at each place a session is created after successful authentication (`btnLogin_Click`, the
auto-submit path in `txtPassword_TextChanged`, and the `ValidateOtp` WebMethod).

```csharp
/// <summary>
/// If the selected ERP role is a Time-Management role, hand off to the TM app via a
/// one-time token and return true (caller must stop — a redirect was issued).
/// Returns false for non-TM roles so normal ERP login proceeds.
/// </summary>
static bool TryStartTimeManagement(string userName, string roleId)
{
    var objDB = new dbaccess();

    // Canonical role name for the selected role id.
    string roleName = objDB.execute_scalar(
        "SELECT ROLE_NAME FROM AMM_ROLE_DETAILS WHERE ROLE_ID = '" + roleId + "'");

    if (roleName != "TimeManagement" && roleName != "TM-Admin" && roleName != "TM-SiteAdmin")
        return false;   // not a TM role — let the ERP handle it normally.

    // USER_ID for the authenticated user.
    string userId = objDB.execute_scalar(
        "SELECT USER_ID FROM AMM_USER_DETAILS WHERE UPPER(USER_NAME) = UPPER('" + userName + "')");
    if (string.IsNullOrEmpty(userId))
        return false;

    // One-time token, valid ~2 minutes. Prefer a bind-variable command in the real code
    // (shown parameterised below); execute_non_query with concatenation also works since
    // token/userId/roleName are server-controlled, not user input.
    string token = Guid.NewGuid().ToString("N");

    using (var conn = new OracleConnection(/* ERP Oracle connection string */))
    using (var cmd = conn.CreateCommand())
    {
        conn.Open();
        cmd.BindByName = true;
        cmd.CommandText =
            @"INSERT INTO TM_AUTO_LOGIN_TOKEN
                  (TAT_TOKEN, TAT_USER_ID, TAT_ERP_ROLE_NAME, TAT_USED_YN, TAT_EXPIRY_DATE)
              VALUES (:token, :userId, :roleName, 'N', SYSDATE + (2/1440))";
        cmd.Parameters.Add(":token",    token);
        cmd.Parameters.Add(":userId",   int.Parse(userId));
        cmd.Parameters.Add(":roleName", roleName);
        cmd.ExecuteNonQuery();
    }

    string tmBase = ConfigurationManager.AppSettings["TM_BASE_URL"];
    HttpContext.Current.Response.Redirect(tmBase + "/Authentication/AutoLogin?token=" + token, false);
    return true;
}
```

### Call site — `btnLogin_Click` (and equivalents)

```csharp
if (ValidateADUser())
{
    // NEW: divert TM roles to the Time-Management app before creating the ERP session.
    if (TryStartTimeManagement(HiddenField2.Value.Trim(), ddlRole.SelectedValue))
        return;

    if (CreateSession(HiddenField2.Value.Trim(), ddlCompany.SelectedValue, ddlRole.SelectedValue, chkRemember.Checked))
        Response.Redirect(@"~/Home.aspx");
    // ...existing else-branch unchanged...
}
```

Apply the same `TryStartTimeManagement(...) → return;` guard before the other two
`CreateSession(...) → Home.aspx` spots (the single-role auto-submit in `txtPassword_TextChanged`,
and after OTP validation in `ValidateOtp`).

## Sign-out — return to the ERP central login

Because the ERP is now the single entry point, signing out of Time Management sends the user
back to the ERP's central login rather than the TM login page. This is parameterised on the
**TM side** via `appsettings.json`:

```json
"TimeManagement": {
  "ErpLoginUrl": "https://your-erp-host/YesERP/Login.aspx"
}
```

`Features/Authentication/Logout` reads this key and redirects there after clearing the auth
cookie. When it is left blank (e.g. in development) sign-out falls back to the local
`/Authentication/Login` page, so nothing breaks before the URL is configured.

## Notes

- The token is single-use and expires in ~2 minutes; a reused/expired link sends the user to the
  TM login page with a message (manual login still works as a fallback).
- Nothing in the ERP session model changes for non-TM roles.
- If you later want the ERP role to always override the TM role, change the TM side
  (`UserRepository.EnsureRoleAsync`) — today it intentionally only creates a role when the user
  has none, so SITEADMIN assignments in TM are preserved.
