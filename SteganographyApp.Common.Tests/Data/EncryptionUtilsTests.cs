namespace SteganographyApp.Common.Tests
{
    using System.Text;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class EncryptionUtilTests : FixtureWithTestObjects
    {
        private const string InputString = "Testing123!@#";
        private const string Password = "Pass1";
        private const int AdditionalHashIterations = 2;

        private readonly byte[] inputStringBytes = Encoding.UTF8.GetBytes(InputString);

        [Test]
        public void TestEncryptAndDecrypt()
        {
            var util = new EncryptionUtil();

            byte[] encrypted = util.Encrypt(inputStringBytes, Password, AdditionalHashIterations);

            Assert.AreNotEqual(inputStringBytes, encrypted);

            byte[] decrypted = util.Decrypt(encrypted, Password, AdditionalHashIterations);

            Assert.AreEqual(inputStringBytes, decrypted);
        }
    }
}