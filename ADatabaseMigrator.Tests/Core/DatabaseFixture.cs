using ADatabaseFixture;

namespace ADatabaseMigrator.Tests.Core;

public class DatabaseFixture() : DatabaseFixtureBase(new SqlServerDatabaseAdapter($"ADatabaseMigrator_Tests_{DateTime.Now:yyyy-MM-dd_HH-mm}")), IAsyncLifetime { }
