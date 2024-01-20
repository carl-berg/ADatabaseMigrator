using System.Collections.Generic;
using ADatabaseMigrator.Core;

namespace ADatabaseMigrator
{
    public class Journal : MigrationJournal<Migration>
    {
        public Journal(IReadOnlyList<Migration> migrations) : base(migrations) { }

        public bool HasChanged(Migration migration)
        {
            if (Migrations.TryGetValue(migration.Id, out var journaledMigration))
            {
                return Equals(journaledMigration.ScriptHash, migration.ScriptHash) is false;
            }

            return true;
        }
    }
}
