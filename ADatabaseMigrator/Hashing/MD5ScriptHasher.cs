using System;
using System.Security.Cryptography;
using System.Text;

namespace ADatabaseMigrator.Hashing;

public class MD5ScriptHasher : IScriptHasher
{
    private static MD5CryptoServiceProvider _cryptoServiceProvider = new();

    public string Hash(string script)
    {
        var scriptBytes = Encoding.UTF8.GetBytes(script);
        var hashBytes = _cryptoServiceProvider.ComputeHash(scriptBytes);
        return Format(hashBytes);
    }

    public virtual string Format(byte[] hash) => BitConverter
        .ToString(hash)
        .Replace("-", string.Empty)
        .ToLower();
}
