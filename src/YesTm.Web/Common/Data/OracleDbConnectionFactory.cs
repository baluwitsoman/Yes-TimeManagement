using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace YesTm.Web.Common.Data;

/// <summary>
/// ODP.NET (Oracle.ManagedDataAccess.Core) implementation of <see cref="IDbConnectionFactory"/>.
/// Note: ODP.NET binds parameters positionally by default, so repository SQL lists named
/// parameters (:p) in the same order as the properties of the Dapper parameter object.
/// </summary>
public sealed class OracleDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<OracleDbConnectionFactory> _logger;

    public OracleDbConnectionFactory(IConfiguration configuration, ILogger<OracleDbConnectionFactory> logger)
    {
        _connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Connection string 'Oracle' is not configured.");
        _logger = logger;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = new OracleConnection(_connectionString);
        try
        {
            await connection.OpenAsync(ct);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Oracle connection");
            await connection.DisposeAsync();
            throw;
        }
    }
}
