namespace SteganographyApp.Common.Data
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Contract for interacting with the EncryptionUtil instance.
    /// </summary>
    public interface IEncryptionUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
        string Encrypt(string value, string password);

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Decrypt/*' />
        string Decrypt(string value, string password);
    }

    /// <summary>
    /// Utility class for encrypting and decrypting content.
    /// </summary>
    [Injectable(typeof(IEncryptionUtil))]
    public sealed class EncryptionUtil : IEncryptionUtil
    {
        private const int Iterations = 10000;
        private const int KeySize = 256;
        private const int IvSize = 16;

        /// <include file='../docs.xml' path='docs/members[@name="EncryptionUtil"]/Encrypt/*' />
        public string Encrypt(string value, string password)
        {
            var keyBytes = GenerateKey(password);
            var iv = GenerateRandomBytes(IvSize);
            using (var managed = new RijndaelManaged { Mode = CipherMode.CBC })
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
            var valueBytes = Convert.FromBase64String(value);
            var keyBytes = GenerateKey(password);
            using (var managed = new RijndaelManaged { Mode = CipherMode.CBC })
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

        private static byte[] GenerateKey(string password)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var salt = Pbkdf2(passwordBytes, Sha512(password + password.Length), Iterations, KeySize);
            return Pbkdf2(passwordBytes, salt, Iterations, KeySize / 8);
        }

        private static byte[] Pbkdf2(byte[] data, byte[] salt, int iterations, int size)
        {
            return new Rfc2898DeriveBytes(data, salt, iterations).GetBytes(size);
        }

        private static byte[] Sha512(string data)
        {
            return SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static byte[] GenerateRandomBytes(int size)
        {
            var bytes = new byte[size];
            RandomNumberGenerator.Create().GetBytes(bytes);
            return bytes;
        }
    }
}