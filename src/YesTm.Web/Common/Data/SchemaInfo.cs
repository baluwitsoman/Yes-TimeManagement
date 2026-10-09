using System.Collections.Concurrent;
using System.Data;
using Dapper;

namespace YesTm.Web.Common.Data;

/// <summary>
/// Lets code that depends on an additive migration keep working on a database where that script has
/// not been run yet (e.g. DT_STD_HOURS_PER_DAY from db/10). "Exists" is cached for good; "missing" is
/// re-checked every few minutes, so running the script is picked up without a restart.
/// </summary>
public static class SchemaInfo
{
    private static readonly ConcurrentDictionary<string, (bool Exists, DateTime CheckedUtc)> Cache = new();
    private static readonly TimeSpan RecheckMissing = TimeSpan.FromMinutes(5);

    public static async Task<bool> HasColumnAsync(IDbConnection conn, string table, string column, CancellationToken ct = default)
    {
        var key = $"{table}.{column}".ToUpperInvariant();
        if (Cache.TryGetValue(key, out var hit) && (hit.Exists || DateTime.UtcNow - hit.CheckedUtc < RecheckMissing))
            return hit.Exists;

        var n = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            new { t = table.ToUpperInvariant(), c = column.ToUpperInvariant() }, cancellationToken: ct));
        Cache[key] = (n > 0, DateTime.UtcNow);
        return n > 0;
    }
}
