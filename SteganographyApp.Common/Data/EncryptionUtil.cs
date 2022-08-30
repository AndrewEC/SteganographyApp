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
        string Encrypt(string value, string password);

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Decrypt/*' />
        string Decrypt(string value, string password);

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

        private readonly ILogger log = new LazyLogger<EncryptionUtil>();

        private const int KeySize = 256;
        private const int IvSize = 16;

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
        public string Encrypt(string value, string password)
        {
            log.Debug("Encrypting value using key: [{0}]", password);
            var keyBytes = GenerateKey(password, DefaultIterations);
            var iv = GenerateRandomBytes(IvSize);
            using (var managed = Aes.Create("AesManaged")!)
            {
                var cryptor = managed.CreateEncryptor(keyBytes, iv);
                byte[] ciphertext;
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, cryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
                        cs.FlushFinalBlock();
                    }

                    ciphertext = ms.ToArray();
                }

                return Convert.ToBase64String(ciphertext);
            }
        }

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Decrypt/*' />
        public string Decrypt(string value, string password)
        {
            log.Debug("Decrypting value using key: [{0}]", password);
            var valueBytes = Convert.FromBase64String(value);
            var keyBytes = GenerateKey(password, DefaultIterations);
            using (var managed = Aes.Create("AesManaged")!)
            {
                using (var ms = new MemoryStream(valueBytes))
                {
                    var iv = new byte[IvSize];
                    ms.Read(iv, 0, iv.Length);

                    var decryptor = managed.CreateDecryptor(keyBytes, iv);

                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs, Encoding.UTF8))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/GenerateKey/*' />
        public byte[] GenerateKey(string value, int iterations)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(value);
            var salt = Pbkdf2(passwordBytes, Sha512(value + value.Length), iterations, KeySize);
            return Pbkdf2(passwordBytes, salt, iterations, KeySize / 8);
        }

        private static byte[] Pbkdf2(byte[] data, byte[] salt, int iterations, int size) => new Rfc2898DeriveBytes(data, salt, iterations).GetBytes(size);

        private static byte[] Sha512(string data) => SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(data));

        private static byte[] GenerateRandomBytes(int size)
        {
            var bytes = new byte[size];
            RandomNumberGenerator.Create().GetBytes(bytes);
            return bytes;
        }
    }
}