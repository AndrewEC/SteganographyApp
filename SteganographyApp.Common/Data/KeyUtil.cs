namespace SteganographyApp.Common.Data;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using SteganographyApp.Common.Logging;

/// <summary>
/// A contract for interacting with the KeyUtil.
/// </summary>
public interface IKeyUtil
{
    /// <summary>
    /// Generates a salted and hashed key from the input value.
    /// </summary>
    /// <param name="password">The value to be salted and hashed.</param>
    /// <param name="additionalIterations">The additional number of times the password will be hashed.
    /// This is in addition to the default number of iterations of 450,000.</param>
    /// <returns>A byte representation of the salted and hashed key derived from the value param.</returns>
    byte[] GenerateKey(string password, int additionalIterations);
}

/// <summary>
/// The IKeyUtil implementation. This implements the logic for generating and caching
/// various salted keys.
/// </summary>
public class KeyUtil : IKeyUtil
{
    private const int DefaultIterations = 450_000;
    private const string CacheKeyTemplate = "{0}-{1}";
    private const int KeySize = 256;

    private readonly Dictionary<string, byte[]> generatedKeys = [];
    private readonly ILogger log = new LazyLogger<KeyUtil>();

    /// <inheritdoc/>
    public byte[] GenerateKey(string password, int additionalIterations)
    {
        string cacheKeyName = string.Format(CacheKeyTemplate, password, additionalIterations);
        if (generatedKeys.TryGetValue(cacheKeyName, out byte[]? cachedKey))
        {
            return cachedKey;
        }

        return generatedKeys[cacheKeyName] = DoGenerateNewKey(password, additionalIterations);
    }

    private static byte[] Pbkdf2(byte[] data, byte[] salt, int iterations, int size)
        => new Rfc2898DeriveBytes(data, salt, iterations, HashAlgorithmName.SHA512).GetBytes(size);

    private static byte[] Sha512(string data)
        => SHA512.HashData(Encoding.UTF8.GetBytes(data));

    private byte[] DoGenerateNewKey(string password, int additionalIterations)
    {
        int iterations = DefaultIterations + additionalIterations;
        log.Debug("Generating key from value [{0}] over [{1}] iterations.", password, iterations);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = Pbkdf2(passwordBytes, Sha512(password + password.Length), iterations, KeySize);
        return Pbkdf2(passwordBytes, salt, iterations, KeySize / 8);
    }
}