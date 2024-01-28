using System.Collections.Generic;

namespace ADatabaseMigrator.Core;

public interface IMigrationJournal<TMigration> : IEnumerable<TMigration> where TMigration : IMigration
{
    bool Contains(IMigration migration);
}
