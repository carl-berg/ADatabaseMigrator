using ADatabaseFixture;
using Microsoft.Data.SqlClient;

namespace ADatabaseMigrator.Tests.Core;

public class DatabaseFixture() : DatabaseFixtureBase(
    new SqlServerDatabaseAdapter(
        connectionFactory: ConnectionFactory,
        databaseName: $"ADatabaseMigrator_Tests_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}")), IAsyncLifetime
{
    public static SqlConnection ConnectionFactory(string connectionString) => new(connectionString);
}
