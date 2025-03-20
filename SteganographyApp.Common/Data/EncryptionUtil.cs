namespace SteganographyApp.Common.Data;

using System;
using System.IO;
using System.Security.Cryptography;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// Contract for interacting with the EncryptionUtil instance.
/// </summary>
public interface IEncryptionUtil
{
    /// <summary>
    /// Encrypts the provided base64 encoded string using the input password.
    /// </summary>
    /// <param name="value">The base64 encoded string to encrypt.</param>
    /// <param name="password">The password to use in the AES cypher.</param>
    /// <param name="additionalPasswordHashIterations">The additional number of times the password should be hashed before being encrypting or decrypting content.
    /// Has no effect if no password has been provided.</param>
    /// <returns>An encrypted base64 encoded string.</returns>
    byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations);

    /// <summary>
    /// Decrypts the provided base64 encoded string using the input password.
    /// </summary>
    /// <param name="value">The base64 encoded string to decrypt.</param>
    /// <param name="password">The password to use in the AES cypher.</param>
    /// <param name="additionalPasswordHashIterations">
    /// The additional number of times the password should be hashed before being encrypting or decrypting content.
    /// Has no effect if no password has been provided.</param>
    /// <returns>A decrypted base64 encoded string.</returns>
    byte[] Decrypt(byte[] value, string password, int additionalPasswordHashIterations);
}

/// <summary>
/// Utility class for encrypting and decrypting content.
/// </summary>
public sealed class EncryptionUtil : IEncryptionUtil
{
    /// <summary>The default number of iterations that should be used when hashing a key.</summary>
    public static readonly int DefaultIterations = 150_000;

    private const int IvSize = 16;

    private readonly ILogger log = new LazyLogger<EncryptionUtil>();

    private readonly IKeyUtil keyUtil;

#pragma warning disable CS1591, SA1600
    public EncryptionUtil(IKeyUtil keyUtil)
    {
        this.keyUtil = keyUtil;
    }

#pragma warning restore CS1591, SA1600

    /// <inheritdoc/>
    public byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations)
    {
        byte[] keyBytes = ServiceContainer.GetService<IKeyUtil>().GenerateKey(password, additionalPasswordHashIterations);
        log.Debug("Encrypting value using key: [{0}]/[{1}]", () => [password, Convert.ToBase64String(keyBytes)]);
        log.Trace("Encrypting value: [{0}]", () => [Convert.ToBase64String(value)]);

        byte[] iv = GenerateRandomBytes(IvSize);

        using (var managed = Aes.Create())
        {
            var cryptor = managed.CreateEncryptor(keyBytes, iv);
            using (var ms = new MemoryStream())
            {
                ms.Write(iv, 0, iv.Length);

                using (var cs = new CryptoStream(ms, cryptor, CryptoStreamMode.Write))
                {
                    cs.Write(value, 0, value.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }
    }

    /// <inheritdoc/>
    public byte[] Decrypt(byte[] value, string password, int additionalPasswordHashIterations)
    {
        byte[] keyBytes = keyUtil.GenerateKey(password, additionalPasswordHashIterations);
        log.Debug("Decrypting value using key: [{0}]", () => [Convert.ToBase64String(keyBytes)]);
        log.Trace("Decrypting value: [{0}]", () => [Convert.ToBase64String(value)]);

        using (var managed = Aes.Create())
        {
            using (var ms = new MemoryStream(value))
            {
                byte[] iv = new byte[IvSize];
                ms.Read(iv, 0, iv.Length);

                var decryptor = managed.CreateDecryptor(keyBytes, iv);

                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var msOut = new MemoryStream())
                    {
                        cs.CopyTo(msOut);
                        return msOut.ToArray();
                    }
                }
            }
        }
    }

    private static byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Create().GetBytes(bytes);
        return bytes;
    }
}