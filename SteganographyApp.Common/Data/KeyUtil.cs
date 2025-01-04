namespace SteganographyApp.Common.Data;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// A contract for interacting with the KeyUtil.
/// </summary>
public interface IKeyUtil
{
    /// <include file='../docs.xml' path='docs/members[@name="KeyUtil"]/GenerateKey/*' />
    byte[] GenerateKey(string password, int additionalIterations);
}

/// <summary>
/// The IKeyUtil implementation. This implements the logic for generating and caching
/// various salted keys.
/// </summary>
[Injectable(typeof(IKeyUtil))]
public class KeyUtil : IKeyUtil
{
    private static readonly int DefaultIterations = 450_000;
    private static readonly string CacheKeyTemplate = "{0}-{1}";
    private static readonly object SyncLock = new();
    private static readonly int KeySize = 256;

    private readonly Dictionary<string, byte[]> generatedKeys = [];
    private readonly ILogger log = new LazyLogger<KeyUtil>();

    /// <include file='../docs.xml' path='docs/members[@name="KeyUtil"]/GenerateKey/*' />
    public byte[] GenerateKey(string password, int additionalIterations)
    {
        string cacheKeyName = string.Format(CacheKeyTemplate, password, additionalIterations);
        lock (SyncLock)
        {
            if (generatedKeys.TryGetValue(cacheKeyName, out byte[]? cachedKey))
            {
                return cachedKey;
            }
            byte[] newKey = DoGenerateNewKey(password, additionalIterations);
            generatedKeys[cacheKeyName] = newKey;
            return newKey;
        }
    }

    private byte[] DoGenerateNewKey(string password, int additionalIterations)
    {
        int iterations = DefaultIterations + additionalIterations;
        log.Debug("Generating key from value [{0}] over [{1}] iterations.", password, iterations);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = Pbkdf2(passwordBytes, Sha512(password + password.Length), iterations, KeySize);
        return Pbkdf2(passwordBytes, salt, iterations, KeySize / 8);
    }

    private static byte[] Pbkdf2(byte[] data, byte[] salt, int iterations, int size) => new Rfc2898DeriveBytes(data, salt, iterations, HashAlgorithmName.SHA512).GetBytes(size);

    private static byte[] Sha512(string data) => SHA512.HashData(Encoding.UTF8.GetBytes(data));

    private static byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Create().GetBytes(bytes);
        return bytes;
    }
}