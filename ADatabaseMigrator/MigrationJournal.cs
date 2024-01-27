using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADatabaseMigrator.Core;

namespace ADatabaseMigrator;

public class MigrationJournal : IMigrationJournal<Migration>
{
    public MigrationJournal(IReadOnlyList<Migration> migrations) 
    {
        Migrations = migrations.ToDictionary(x => x.Name, x => x);
    }

    protected Dictionary<string, Migration> Migrations { get; }

    public bool Contains(IMigration migration) => Migrations.ContainsKey(migration.Name);

    public bool HasChanged(MigrationScript migration)
    {
        if (Migrations.TryGetValue(migration.Name, out var journaledMigration))
        {
            return Equals(journaledMigration.ScriptHash, migration.ScriptHash) is false;
        }

        return true;
    }

    public IEnumerator<Migration> GetEnumerator() => Migrations.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
