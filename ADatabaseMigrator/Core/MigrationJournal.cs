using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ADatabaseMigrator.Core
{
    public class MigrationJournal<TMigration> : IEnumerable<TMigration> where TMigration : IMigration
    {
        public MigrationJournal(IReadOnlyList<TMigration> migrations) => Migrations = migrations.ToDictionary(x => x.Name, x => x);

        protected Dictionary<string, TMigration> Migrations { get; }

        public virtual bool Contains(IMigration migration) => Migrations.ContainsKey(migration.Name);

        public IEnumerator<TMigration> GetEnumerator() => Migrations.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
