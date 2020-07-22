using Rijndael256;

namespace SteganographyApp.Common.Providers
{

    /// <summary>
    /// Provides a wrapper interface for interacting with the Rijndael encryption library.
    /// </summary>
    public interface IEncryptionProvider
    {
        string Encrypt(string base64String, string password);
        string Decrypt(string base64String, string password);
    }

    public class EncryptionProvider : IEncryptionProvider
    {

        public string Encrypt(string base64String, string password)
        {
            return Rijndael.Encrypt(base64String, password, KeySize.Aes256);
        }

        public string Decrypt(string base64String, string password)
        {
            return Rijndael.Decrypt(base64String, password, KeySize.Aes256);
        }

    }

}