namespace ADatabaseMigrator.Hashing;

public interface IScriptHasher
{
    string Hash(string script);
}
