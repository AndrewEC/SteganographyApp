using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class Init
    {

        [OneTimeSetUp]
        public void OneTime()
        {
            Injector.AllowOnlyTestObjects();
        }

    }

    [TestFixture]
    public abstract class FixtureWithMockConsoleReaderAndWriter : FixtureWithTestObjects
    {

        [SetUp]
        public void SetUp()
        {
            Injector.UseInstance(new Mock<IConsoleReader>().Object);
            Injector.UseInstance(new Mock<IConsoleWriter>().Object);
        }

    }

    [TestFixture]
    public abstract class FixtureWithTestObjects
    {

        [TearDown]
        public void TearDown()
        {
            Injector.ResetInstances();
        }

    }

    [TestFixture]
    public abstract class FixtureWithRealObjects
    {

        [SetUp]
        public void SetUp()
        {
            Injector.AllowOnlyRealObjects();
        }

        [TearDown]
        public void TearDown()
        {
            Injector.AllowOnlyTestObjects();
            Injector.ResetInstances();
        }

    }

}