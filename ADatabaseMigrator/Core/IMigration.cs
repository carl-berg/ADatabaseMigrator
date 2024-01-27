namespace ADatabaseMigrator.Core
{
    public interface IMigration
    {
        /// <summary>
        /// Migration name that uniquely identifies this migration
        /// </summary>
        string Name { get; }
    }
}
