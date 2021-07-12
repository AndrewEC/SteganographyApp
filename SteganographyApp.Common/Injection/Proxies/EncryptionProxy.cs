namespace SteganographyApp.Common.Injection
{
    using Rijndael256;

    using SteganographyApp.Common.Logging;

    /// <summary>
    /// A proxy interface for interacting with the Rijndael encryption library.
    /// </summary>
    public interface IEncryptionProxy
    {
        /// <include file='../../docs.xml' path='docs/members[@name="EncryptionProxy"]/Encrypt/*' />
        string Encrypt(string base64String, string password);

        /// <include file='../../docs.xml' path='docs/members[@name="EncryptionProxy"]/Decrypt/*' />
        string Decrypt(string base64String, string password);
    }

    /// <summary>
    /// The concrete encryption provider exposing the Rijndael encrypt and decrypt methods.
    /// </summary>
    [Injectable(typeof(IEncryptionProxy))]
    public class EncryptionProxy : IEncryptionProxy
    {
        private ILogger log;

        /// <summary>
        /// The post construct method to initialize the logger.
        /// </summary>
        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<EncryptionProxy>();
        }

        /// <include file='../../docs.xml' path='docs/members[@name="EncryptionProxy"]/Encrypt/*' />
        public string Encrypt(string base64String, string password)
        {
            log.Debug("Encrypting content using password [{0}]", password);
            return Rijndael.Encrypt(base64String, password, KeySize.Aes256);
        }

        /// <include file='../../docs.xml' path='docs/members[@name="EncryptionProxy"]/Decrypt/*' />
        public string Decrypt(string base64String, string password)
        {
            log.Debug("Decrypting content using password [{0}]", password);
            return Rijndael.Decrypt(base64String, password, KeySize.Aes256);
        }
    }
}