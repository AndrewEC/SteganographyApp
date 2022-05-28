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
    public abstract class FixtureWithTestObjects : FixtureWithLogger
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

        public void Trace(string message, Func<object[]> provider) { }

        public void Debug(string message, params object[] arguments) { }

        public void Debug(string message, Func<object[]> provider) { }

        public void Error(string message, params object[] arguments) { }

        public void Error(string message, Func<object[]> provider) { }

        public void Log(LogLevel level, string message, params object[] arguments) { }

        public void Log(LogLevel level, string message, Func<object[]> provider) { }
    }

    internal class NullLoggerFactory : ILoggerFactory
    {
        public ILogger LoggerFor(Type type)
        {
            return new NullLogger();
        }
    }
}