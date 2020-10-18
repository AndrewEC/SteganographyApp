using System;
using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    class InjectorTests : FixtureWithRealObjects
    {

        [Test]
        public void TestProvideDefaultInjectableInstance()
        {
            var reader = Injector.Provide<IConsoleReader>();
            Assert.True(reader is ConsoleKeyReader);
        }

        [Test]
        public void TestProvideMockedInjectableInstance()
        {
            var expected = new Mock<IConsoleReader>();
            Injector.UseInstance<IConsoleReader>(expected.Object);

            var actual = Injector.Provide<IConsoleReader>();

            Assert.AreEqual(expected.Object, actual);
        }

        [Test]
        public void TestProvideDefaultInstanceWhenOnlyMocksAreAllowedThrowsException()
        {
            Injector.AllowOnlyNonDefaultInstances();

            var actual = Assert.Throws<InvalidOperationException>(() => {
                Injector.Provide<IConsoleReader>();
            });

            Assert.True(actual.Message.Contains(typeof(IConsoleReader).Name));
        }

        [Test]
        public void TestResetInstancesProvidesDefaultInstance()
        {
            var mockReader = new Mock<IConsoleReader>();
            Injector.UseInstance<IConsoleReader>(mockReader.Object);
            
            Injector.ResetInstances();
            var reader = Injector.Provide<IConsoleReader>();

            Assert.AreNotEqual(mockReader.Object, reader);
        }

    }

}