namespace ADatabaseMigrator.Core
{
    public interface IMigration
    {
        /// <summary>
        /// Migration identifier that uniquely identifies this migration
        /// </summary>
        string Name { get; }
    }
}
