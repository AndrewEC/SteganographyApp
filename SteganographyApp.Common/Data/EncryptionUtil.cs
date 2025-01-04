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
    /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
    byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations);

    /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Decrypt/*' />
    byte[] Decrypt(byte[] value, string password, int additionalPasswordHashIterations);
}

/// <summary>
/// Utility class for encrypting and decrypting content.
/// </summary>
[Injectable(typeof(IEncryptionUtil))]
public sealed class EncryptionUtil : IEncryptionUtil
{
    /// <summary>The default number of iterations that should be used when hashing a key.</summary>
    public static readonly int DefaultIterations = 150_000;

    private const int KeySize = 256;
    private const int IvSize = 16;

    private readonly ILogger log = new LazyLogger<EncryptionUtil>();

    /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
    public byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations)
    {
        byte[] keyBytes = Injector.Provide<IKeyUtil>().GenerateKey(password, additionalPasswordHashIterations);
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

    /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Decrypt/*' />
    public byte[] Decrypt(byte[] value, string password, int additionalPasswordHashIterations)
    {
        byte[] keyBytes = Injector.Provide<IKeyUtil>().GenerateKey(password, additionalPasswordHashIterations);
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