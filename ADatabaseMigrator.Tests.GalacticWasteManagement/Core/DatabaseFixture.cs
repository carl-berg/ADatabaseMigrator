using ADatabaseFixture;

namespace ADatabaseMigrator.Tests.GalacticWasteManagement.Core;

public class DatabaseFixture() : DatabaseFixtureBase(new SqlServerDatabaseAdapter()), IAsyncLifetime { }
