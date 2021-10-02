namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class EncryptionUtilTests
    {
        private static readonly string InputString = "Testing123!@#";
        private static readonly string Password = "Pass1";

        [Test]
        public void TestEncryptAndDecrypt()
        {
            var util = new EncryptionUtil();

            var encrypted = util.Encrypt(InputString, Password);

            Assert.AreNotEqual(InputString, encrypted);

            var decrypted = util.Decrypt(encrypted, Password);

            Assert.AreEqual(InputString, decrypted);
        }
    }
}