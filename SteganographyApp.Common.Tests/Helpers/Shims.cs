namespace SteganographyApp.Common.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    [TestFixture]
    public class Init
    {
        [OneTimeSetUp]
        public void OneTime()
        {
            Injector.AllowOnlyMockObjects();
        }
    }

    [TestFixture]
    public abstract class FixtureWithMockConsoleReaderAndWriter : FixtureWithTestObjects
    {
        [Mockup(typeof(IConsoleReader))]
        public Mock<IConsoleReader> mockConsoleReader;

        [Mockup(typeof(IConsoleWriter))]
        public Mock<IConsoleWriter> mockConsoleWriter;
    }

    [TestFixture]
    public abstract class FixtureWithTestObjects
    {
        [SetUp]
        public void SetUp()
        {
            MocksInjector.InjectMocks(this);
            Injector.UseInstance<ILoggerFactory>(new NullLoggerFactory());
            SetupMocks();
        }

        [TearDown]
        public void TearDown()
        {
            Injector.ResetInstances();
        }

        protected virtual void SetupMocks() { }
    }

    [TestFixture]
    public abstract class FixtureWithRealObjects : FixtureWithTestObjects
    {
        [SetUp]
        public new void SetUp()
        {
            Injector.AllowAnyObjects();
            base.SetUp();
        }

        [TearDown]
        public new void TearDown()
        {
            Injector.AllowOnlyMockObjects();
            base.TearDown();
        }
    }

    internal sealed class NullLogger : ILogger
    {
        public void Trace(string message, params object[] arguments) { }

        public void Trace(string message, Func<object[]> provider) { }

        public void Debug(string message, params object[] arguments) { }

        public void Debug(string message, Func<object[]> provider) { }

        public void Error(string message, params object[] arguments) { }

        public void Error(string message, Func<object[]> provider) { }
    }

    internal sealed class NullLoggerFactory : ILoggerFactory
    {
        public ILogger LoggerFor(Type type)
        {
            return new NullLogger();
        }
    }
}