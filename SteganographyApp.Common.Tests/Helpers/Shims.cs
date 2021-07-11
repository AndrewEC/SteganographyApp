namespace SteganographyApp.Common.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Injection;

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
        [SetUp]
        public void SetUp()
        {
            Injector.UseInstance(new Mock<IConsoleReader>().Object);
            Injector.UseInstance(new Mock<IConsoleWriter>().Object);
            Injector.UseInstance<ILoggerFactory>(new NullLoggerFactory());
        }
    }

    [TestFixture]
    public abstract class FixtureWithTestObjects
    {
        [SetUp]
        public void InitializeMocks()
        {
            MocksInjector.InjectMocks(this);
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
    public abstract class FixtureWithRealObjects
    {
        [SetUp]
        public void SetUp()
        {
            Injector.AllowAnyObjects();
        }

        [TearDown]
        public void TearDown()
        {
            Injector.AllowOnlyMockObjects();
            Injector.ResetInstances();
        }
    }

    [TestFixture]
    public abstract class FixtureWithLogger
    {
        [SetUp]
        public void InitializeMockLogger()
        {
            Injector.UseInstance<ILoggerFactory>(new NullLoggerFactory());
        }
    }

    internal class NullLogger : ILogger
    {
        public void Trace(string message, params object[] arguments) { }

        public void Trace(string message, ArgumentProvider provider) { }

        public void Debug(string message, params object[] arguments) { }

        public void Debug(string message, ArgumentProvider provider) { }

        public void Error(string message, params object[] arguments) { }

        public void Error(string message, ArgumentProvider provider) { }

        public void Log(LogLevel level, string message, params object[] arguments) { }

        public void Log(LogLevel level, string message, ArgumentProvider provider) { }
    }

    internal class NullLoggerFactory : ILoggerFactory
    {
        public ILogger LoggerFor(Type type)
        {
            return new NullLogger();
        }
    }
}