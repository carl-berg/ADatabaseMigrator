using Respawn;

namespace ADatabaseMigrator.Tests.Core;

[Collection("DatabaseIntegrationTest")]
public abstract class DatabaseTest : IAsyncLifetime
{
    protected DatabaseTest(DatabaseFixture fixture) => Fixture = fixture;

    protected DatabaseFixture Fixture { get; }
    private static Respawner? Respawner { get; set; }

    public async Task InitializeAsync()
    {
        //Respawner ??= await Respawner.CreateAsync(Fixture.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        //await (Respawner?.ResetAsync(Fixture.ConnectionString) ?? Task.CompletedTask);
    }
}
