using ADatabaseFixture;
using System.Data.SqlClient;

namespace ADatabaseMigrator.Tests.GalacticWasteManagement.Core;

public class DatabaseFixture() : DatabaseFixtureBase(
    new SqlServerDatabaseAdapter(
        connectionFactory: ConnectionFactory,
        databaseName: $"ADatabaseMigrator_Tests_GWM_{DateTime.Now:yyyy-MM-dd_HH-mm}")), IAsyncLifetime 
{
    public static SqlConnection ConnectionFactory(string connectionString) => new(connectionString);
}
