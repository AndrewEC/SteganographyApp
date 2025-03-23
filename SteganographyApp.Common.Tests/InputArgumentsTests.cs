namespace SteganographyApp.Common.Tests;

using NUnit.Framework;

[TestFixture]
public class InputArgumentsTests
{
    [Test]
    public void TestInitialValue()
    {
        CommonArguments arguments = new();

        Assert.Multiple(() =>
        {
            Assert.That(arguments.Password, Is.EqualTo(string.Empty));
            Assert.That(arguments.FileToEncode, Is.EqualTo(string.Empty));
            Assert.That(arguments.DecodedOutputFile, Is.EqualTo(string.Empty));
            Assert.That(arguments.CoverImages, Has.Length.EqualTo(0));
            Assert.That(arguments.UseCompression, Is.False);
            Assert.That(arguments.RandomSeed, Is.EqualTo(string.Empty));
            Assert.That(arguments.DummyCount, Is.EqualTo(0));
            Assert.That(arguments.DeleteAfterConversion, Is.False);
            Assert.That(arguments.ChunkByteSize, Is.EqualTo(131_072));
            Assert.That(arguments.ImageFormat, Is.EqualTo(ImageFormat.Png));
            Assert.That(arguments.AdditionalPasswordHashIterations, Is.EqualTo(0));
            Assert.That(arguments.BitsToUse, Is.EqualTo(1));
        });
    }
}