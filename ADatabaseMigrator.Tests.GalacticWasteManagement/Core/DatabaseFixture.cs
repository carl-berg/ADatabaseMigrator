using ADatabaseFixture;

namespace ADatabaseMigrator.Tests.GalacticWasteManagement.Core;

public class DatabaseFixture() : DatabaseFixtureBase(new SqlServerDatabaseAdapter($"ADatabaseMigrator_Tests_GWM_{DateTime.Now:yyyy-MM-dd_HH-mm}")), IAsyncLifetime { }
