using Microsoft.Data.SqlClient;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlAmbassadorRepository : IAmbassadorRepository
{
    private readonly string _connectionString;

    public SqlAmbassadorRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.CheckReferralCodeExists, connection);
        command.Parameters.AddWithValue("@referralCode", referralCode);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }
}
