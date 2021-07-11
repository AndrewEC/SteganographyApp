namespace SteganographyApp.Common.Injection
{
    using Rijndael256;

    using SteganographyApp.Common.Logging;

    /// <summary>
    /// A proxy interface for interacting with the Rijndael encryption library.
    /// </summary>
    public interface IEncryptionProxy
    {
        string Encrypt(string base64String, string password);

        string Decrypt(string base64String, string password);
    }

    [Injectable(typeof(IEncryptionProxy))]
    public class EncryptionProxy : IEncryptionProxy
    {
        private ILogger log;

        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<EncryptionProxy>();
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