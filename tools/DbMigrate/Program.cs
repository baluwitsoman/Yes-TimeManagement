using System.Text.Json;
using Oracle.ManagedDataAccess.Client;

// ---------------------------------------------------------------------------
// Tiny SQL script runner for the TM_* schema (no sqlplus required).
// Usage:  dotnet run --project tools/DbMigrate [connectionString] [file1.sql file2.sql ...]
// Defaults: connection from src/YesTm.Web/appsettings.json, scripts db/01_*.sql + db/02_*.sql
// Splitting rules: statements separated by a line containing only '/'; otherwise by ';'.
// PL/SQL blocks (DECLARE/BEGIN) are executed whole.
// ---------------------------------------------------------------------------

string? connFromArg = args.FirstOrDefault(a => a.Contains("Data Source", StringComparison.OrdinalIgnoreCase));
var files = args.Where(a => a.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)).ToArray();
if (files.Length == 0)
    files = ["db/01_tm_schema.sql", "db/02_seed.sql"];

var connStr = connFromArg ?? ReadConnectionString("src/YesTm.Web/appsettings.json");
if (string.IsNullOrWhiteSpace(connStr))
{
    Console.Error.WriteLine("No connection string found.");
    return 2;
}

Console.WriteLine($"Connecting: {Mask(connStr)}");
await using var conn = new OracleConnection(connStr);
try
{
    await conn.OpenAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"CONNECT FAILED: {ex.Message}");
    return 3;
}
Console.WriteLine("Connected.\n");

// --query "SELECT ..."  -> run a reader and print rows, then exit.
var qIdx = Array.IndexOf(args, "--query");
if (qIdx >= 0 && qIdx + 1 < args.Length)
{
    await using var qcmd = conn.CreateCommand();
    qcmd.CommandText = args[qIdx + 1];
    await using var r = await qcmd.ExecuteReaderAsync();
    var cols = Enumerable.Range(0, r.FieldCount).Select(r.GetName).ToArray();
    Console.WriteLine(string.Join(" | ", cols));
    while (await r.ReadAsync())
        Console.WriteLine(string.Join(" | ", Enumerable.Range(0, r.FieldCount)
            .Select(i => r.IsDBNull(i) ? "(null)" : r.GetValue(i).ToString())));
    return 0;
}

int ok = 0, skipped = 0, failed = 0;
foreach (var file in files)
{
    if (!File.Exists(file)) { Console.Error.WriteLine($"  ! missing: {file}"); continue; }
    Console.WriteLine($"== {file} ==");
    foreach (var stmt in SplitStatements(await File.ReadAllTextAsync(file)))
    {
        var label = stmt.Replace('\n', ' ').Trim();
        label = label.Length > 70 ? label[..70] + "…" : label;
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt;
            await cmd.ExecuteNonQueryAsync();
            ok++;
            Console.WriteLine($"  ok   {label}");
        }
        catch (OracleException ex) when (ex.Number is 955 or 2260 or 1 or 2275 or 1430 or 1408)
        {
            // 955 name in use, 2260/2275 PK/FK exists, 1 unique violation (seed re-run), etc.
            skipped++;
            Console.WriteLine($"  skip ({ex.Number}) {label}");
        }
        catch (Exception ex)
        {
            failed++;
            Console.Error.WriteLine($"  FAIL {label}\n        {ex.Message}");
        }
    }
    Console.WriteLine();
}

Console.WriteLine($"Done. executed={ok} skipped(existing)={skipped} failed={failed}");
return failed > 0 ? 1 : 0;

// --- helpers ---------------------------------------------------------------
static IEnumerable<string> SplitStatements(string script)
{
    // First split on lines that are just '/'
    var parts = System.Text.RegularExpressions.Regex.Split(script, @"(?m)^\s*/\s*$");
    foreach (var part in parts)
    {
        var trimmed = StripComments(part).Trim();
        if (trimmed.Length == 0) continue;

        var upper = trimmed.ToUpperInvariant();
        if (upper.StartsWith("DECLARE") || upper.StartsWith("BEGIN"))
        {
            yield return trimmed;   // PL/SQL block — run whole
            continue;
        }

        foreach (var s in trimmed.Split(';'))
        {
            var st = s.Trim();
            if (st.Length == 0) continue;
            if (StripComments(st).Trim().Length == 0) continue;
            yield return st;
        }
    }
}

static string StripComments(string sql)
{
    var lines = sql.Split('\n')
        .Where(l => !l.TrimStart().StartsWith("--"));
    return string.Join('\n', lines);
}

static string? ReadConnectionString(string appsettingsPath)
{
    if (!File.Exists(appsettingsPath)) return null;
    using var doc = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
    return doc.RootElement.TryGetProperty("ConnectionStrings", out var cs)
        && cs.TryGetProperty("Oracle", out var o) ? o.GetString() : null;
}

static string Mask(string c) =>
    System.Text.RegularExpressions.Regex.Replace(c, @"(?i)Password=[^;]*", "Password=****");
