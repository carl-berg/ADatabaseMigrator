using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core;

public interface IMigrationJournal<TMigration> : IEnumerable<TMigration> where TMigration : IMigration
{
    Task Add<TMigrationScript>(TMigrationScript script) where TMigrationScript : IMigrationScript;
    bool Contains(IMigration migration);
}
