using ADatabaseFixture;

namespace ADatabaseMigrator.Tests.Core;

public class DatabaseFixture() : DatabaseFixtureBase(new SqlServerDatabaseAdapter()), IAsyncLifetime { }
