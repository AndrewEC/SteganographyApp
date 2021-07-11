namespace SteganographyApp.Common.Injection
{
    using Rijndael256;

    /// <summary>
    /// Provides a wrapper interface for interacting with the Rijndael encryption library.
    /// </summary>
    public interface IEncryptionProvider
    {
        string Encrypt(string base64String, string password);

        string Decrypt(string base64String, string password);
    }

    [Injectable(typeof(IEncryptionProvider))]
    public class EncryptionProvider : IEncryptionProvider
    {
        private ILogger log;

        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<EncryptionProvider>();
        }

        public string Encrypt(string base64String, string password)
        {
            log.Debug("Encrypting content using password [{0}]", password);
            return Rijndael.Encrypt(base64String, password, KeySize.Aes256);
        }

        public string Decrypt(string base64String, string password)
        {
            log.Debug("Decrypting content using password [{0}]", password);
            return Rijndael.Decrypt(base64String, password, KeySize.Aes256);
        }
    }
}