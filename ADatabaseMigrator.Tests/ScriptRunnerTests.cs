using ADatabaseMigrator.SqlServer;
using ADatabaseMigrator.Tests.Core;
using Dapper;
using Shouldly;

namespace ADatabaseMigrator.Tests;

public class ScriptRunnerTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_ScriptRunner_Execution()
    {
        using var connection = Fixture.CreateNewConnection();
        var scriptRunner = new MigrationScriptRunner(connection);

        var script = new MigrationScript(
            name: "My script",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script:
                """
                CREATE TABLE ScriptRunnerTest_Log(
                    Id INT NOT NULL PRIMARY KEY,
                    Log NVARCHAR(50) NOT NULL
                )
                """,
            scriptHash: "hash");

        var appendJournalScript = "INSERT INTO ScriptRunnerTest_Log(Id, Log) VALUES(1, 'Journal_insert')";
        await scriptRunner.Run(script, appendJournalScript, CancellationToken.None);

        // Verify script and log was excuted
        var logs = await connection.QueryAsync<(int Id, string Log)>("SELECT * FROM ScriptRunnerTest_Log");
        logs.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            log => log.Id.ShouldBe(1),
            log => log.Log.ShouldBe("Journal_insert"));
    }

    [Fact]
    public async Task Test_Enlisted_Transaction_Handling()
    {
        using var connection = Fixture.CreateNewConnection();
        using var transaction = connection.BeginTransaction();
        var scriptRunner = new MigrationScriptRunner(connection, transaction);

        var script = new MigrationScript(
            name: "My script",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script:
                """
                CREATE TABLE ScriptRunnerTest_Log(
                    Id INT NOT NULL PRIMARY KEY,
                    Log NVARCHAR(50) NOT NULL
                )
                """,
            scriptHash: "hash");

        var appendJournalScript = "INSERT INTO ScriptRunnerTest_Log(Id, Log) VALUES(1, 'Journal_insert')";
        await scriptRunner.Run(script, appendJournalScript, CancellationToken.None);

        await transaction.RollbackAsync();

        // Verify no table was created
        var numberOfMatchingTables = await connection.QuerySingleAsync<int>(
            """
            SELECT COUNT(1) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = 'dbo' 
            AND TABLE_NAME = 'ScriptRunnerTest_Log'
            """);

        numberOfMatchingTables.ShouldBe(0);
    }

    [Fact]
    public async Task Test_Batched_ScriptRunner_Execution()
    {
        using var connection = Fixture.CreateNewConnection();
        var scriptRunner = new SqlServerMigrationScriptRunner(connection);

        var script = new MigrationScript(
            name: "My script",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script:
                """
                CREATE TABLE Table_A(
                    Id INT NOT NULL PRIMARY KEY,
                    Name NVARCHAR(50) NOT NULL
                )
                GO

                CREATE TABLE Table_B(
                    Id INT NOT NULL PRIMARY KEY,
                    Name NVARCHAR(50) NOT NULL
                )
                GO

                INSERT INTO Table_A(Id, Name) VALUES(1, 'A')
                INSERT INTO Table_B(Id, Name) VALUES(1, 'B')
                """,
            scriptHash: "hash");

        var appendJournalScript = "SELECT 0";
        await scriptRunner.Run(script, appendJournalScript, CancellationToken.None);

        // Verify execution
        var results = await connection.QueryAsync<(int Id, string Name)>(
            """
            SELECT Id, Name FROM Table_A 
            UNION 
            SELECT Id, Name FROM Table_B
            """);

        results.ShouldSatisfyAllConditions(
            rows => rows.Count().ShouldBe(2),
            rows => rows.ShouldContain(x => x.Name == "A"),
            rows => rows.ShouldContain(x => x.Name == "B"));
    }
}
