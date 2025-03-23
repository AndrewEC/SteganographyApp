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
    /// Encrypts the value byte array using AES. This method should not be invoked
    /// if the password is an empty string.
    /// </summary>
    /// <param name="value">The byte data to be encrypted.</param>
    /// <param name="password">The password. Must not be an empty string.</param>
    /// <param name="additionalPasswordHashIterations">The additional number of
    /// times the password should be hashed.</param>
    /// <returns>The byte representation of the AES encrypted value.</returns>
    byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations);

    /// <summary>
    /// Decrypts the input value byte array using standard AES encryption. This method
    /// should not be invoked if the password is an empty string.
    /// </summary>
    /// <param name="value">The bytes to be decrypted.</param>
    /// <param name="password">The password. Must not be empty.</param>
    /// <param name="additionalPasswordHashIterations">
    /// The additional number of times the password should be hashed.</param>
    /// <returns>The unencrypted bytes.</returns>
    byte[] Decrypt(byte[] value, string password, int additionalPasswordHashIterations);
}

/// <summary>
/// Utility class for encrypting and decrypting content.
/// </summary>
public sealed class EncryptionUtil(IKeyUtil keyUtil) : IEncryptionUtil
{
    /// <summary>
    /// The size, in bytes of the initialization vector (iv).
    /// </summary>
    private const int IvSize = 16;

    private readonly ILogger log = new LazyLogger<EncryptionUtil>();

    private readonly IKeyUtil keyUtil = keyUtil;

    /// <inheritdoc/>
    public byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations)
    {
        byte[] keyBytes = ServiceContainer.GetService<IKeyUtil>()
            .GenerateKey(password, additionalPasswordHashIterations);

        log.Debug(
            "Encrypting value using key: [{0}]/[{1}]",
            () => [password, Convert.ToBase64String(keyBytes)]);

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