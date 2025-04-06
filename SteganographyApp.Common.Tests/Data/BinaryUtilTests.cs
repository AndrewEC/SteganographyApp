namespace SteganographyApp.Common.Tests;

using NUnit.Framework;

using SteganographyApp.Common.Data;

[TestFixture]
public class BinaryUtilTests
{
    private const string BinaryString = "0000000100000010";
    private readonly BinaryUtil util = new();

    [Test]
    public void TestToBinaryString()
    {
        string result = util.ToBinaryString(new byte[] { 1, 2 });
        Assert.That(result, Is.EqualTo(BinaryString));
    }

    [Test]
    public void TestBinaryToStringDirect()
    {
        string result = util.ToBinaryStringDirect(new byte[] { 1, 0, 1 });
        Assert.That(result, Is.EqualTo("101"));
    }

    [Test]
    public void TestToBytes()
    {
        byte[] result = util.ToBytes(BinaryString);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(2));
        });
    }

    [Test]
    public void TestToBytesDirect()
    {
        byte[] result = util.ToBytesDirect("101");
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(1));
            Assert.That(result[1], Is.EqualTo(0));
            Assert.That(result[2], Is.EqualTo(1));
        });
    }
}