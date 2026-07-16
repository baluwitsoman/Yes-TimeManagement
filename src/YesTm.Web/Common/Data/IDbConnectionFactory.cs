using System.Data;

namespace YesTm.Web.Common.Data;

/// <summary>
/// Creates open ADO.NET connections to the Oracle database.
/// Repositories depend on this abstraction so the provider can be swapped/mocked.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Opens a new connection. Caller owns disposal.</summary>
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
