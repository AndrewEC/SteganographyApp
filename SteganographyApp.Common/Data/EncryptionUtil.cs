namespace SteganographyApp.Common.Data
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

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

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/GenerateKey/*' />
        byte[] GenerateKey(string value, int iterations);
    }

    /// <summary>
    /// Utility class for encrypting and decrypting content.
    /// </summary>
    [Injectable(typeof(IEncryptionUtil))]
    public sealed class EncryptionUtil : IEncryptionUtil
    {
        /// <summary>The default number of iterations that should be used when hashing a key.</summary>
        public static readonly int DefaultIterations = 50_000;

        private const int KeySize = 256;
        private const int IvSize = 16;

        private readonly ILogger log = new LazyLogger<EncryptionUtil>();

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
        public byte[] Encrypt(byte[] value, string password, int additionalPasswordHashIterations)
        {
            var keyBytes = GenerateKey(password, DefaultIterations + additionalPasswordHashIterations);
            log.Debug("Encrypting value using key: [{0}]/[{1}]", () => new object[] { password, Convert.ToBase64String(keyBytes) });
            var iv = GenerateRandomBytes(IvSize);
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
            var keyBytes = GenerateKey(password, DefaultIterations + additionalPasswordHashIterations);
            log.Debug("Decrypting value using key: [{0}]", () => new[] { Convert.ToBase64String(keyBytes) });
            log.Trace("Decrypting: [{0}]", Convert.ToBase64String(value));

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

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/GenerateKey/*' />
        public byte[] GenerateKey(string value, int iterations)
        {
            log.Debug("Generating key from value [{0}] over [{1}] iterations.", value, iterations);
            var passwordBytes = Encoding.UTF8.GetBytes(value);
            var salt = Pbkdf2(passwordBytes, Sha512(value + value.Length), iterations, KeySize);
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
}