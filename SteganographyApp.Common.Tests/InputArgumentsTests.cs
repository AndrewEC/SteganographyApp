namespace SteganographyApp.Common.Tests;

using NUnit.Framework;

[TestFixture]
public class InputArgumentsTests
{
    [Test]
    public void TestInitialValue()
    {
        CommonArguments arguments = new();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(arguments.Password, Is.EqualTo(string.Empty));
            Assert.That(arguments.FileToEncode, Is.EqualTo(string.Empty));
            Assert.That(arguments.DecodedOutputFile, Is.EqualTo(string.Empty));
            Assert.That(arguments.CoverImages, Is.Empty);
            Assert.That(arguments.UseCompression, Is.False);
            Assert.That(arguments.RandomSeed, Is.EqualTo(string.Empty));
            Assert.That(arguments.DummyCount, Is.Zero);
            Assert.That(arguments.DeleteAfterConversion, Is.False);
            Assert.That(arguments.ChunkByteSize, Is.EqualTo(131_072));
            Assert.That(arguments.ImageFormat, Is.EqualTo(ImageFormat.Png));
            Assert.That(arguments.AdditionalPasswordHashIterations, Is.Zero);
            Assert.That(arguments.BitsToUse, Is.EqualTo(1));
        }
    }
}