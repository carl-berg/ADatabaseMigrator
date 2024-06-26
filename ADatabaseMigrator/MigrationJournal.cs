﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADatabaseMigrator.Core;

namespace ADatabaseMigrator;

public class MigrationJournal(IReadOnlyList<Migration> migrations) : IMigrationJournal<Migration>
{
    protected Dictionary<string, IReadOnlyList<Migration>> Migrations { get; } = migrations
        .GroupBy(x => x.Name)
        .ToDictionary(x => x.Key, x => (IReadOnlyList<Migration>)[.. x.OrderBy(x => x.Applied)]);

    public virtual bool Contains(IMigration migration) => Migrations.ContainsKey(migration.Name);

    public virtual bool HasChanged(MigrationScript migration)
    {
        // Find all migrations matching by name
        if (Migrations.TryGetValue(migration.Name, out var journaledMigrations))
        {
            // Compare with last matching one
            if (journaledMigrations.LastOrDefault() is { } lastMigration)
            {
                return Equals(lastMigration.ScriptHash, migration.ScriptHash) is false;
            }
        }

        return true;
    }

    public IEnumerator<Migration> GetEnumerator() => Migrations.Values.SelectMany(x => x).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
