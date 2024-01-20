namespace ADatabaseMigrator.Core
{
    public interface IMigrationScript : IMigration
    {
        /// <summary>
        /// Script contents to run
        /// </summary>
        string Script { get; }
    }
}
