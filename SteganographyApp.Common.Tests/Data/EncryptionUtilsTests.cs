namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class EncryptionUtilTests : FixtureWithTestObjects
    {
        private const string InputString = "Testing123!@#";
        private const string Password = "Pass1";
        private const int AdditionalHashIterations = 2;

        [Test]
        public void TestEncryptAndDecrypt()
        {
            var util = new EncryptionUtil();

            var encrypted = util.Encrypt(InputString, Password, AdditionalHashIterations);

            Assert.AreNotEqual(InputString, encrypted);

            var decrypted = util.Decrypt(encrypted, Password, AdditionalHashIterations);

            Assert.AreEqual(InputString, decrypted);
        }
    }
}