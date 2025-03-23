namespace SteganographyApp.Common.Tests;

using System;
using NUnit.Framework;

[TestFixture]
public class AbstractDisposableTest
{
    [Test]
    public void TestAbstractDisposable()
    {
        TestClass instance = new();

        using (instance)
        {
            instance.DoWork();
        }

        ObjectDisposedException actual = Assert.Throws<ObjectDisposedException>(
            () => instance.DoWork());

        Assert.Multiple(() =>
        {
            Assert.That(actual.Message, Contains.Substring(instance.GetType().FullName!));
            Assert.That(instance.DisposeCallCount, Is.EqualTo(1));
            Assert.That(instance.DoWorkCallCount, Is.EqualTo(1));
        });
    }

    internal sealed class TestClass : AbstractDisposable
    {
        public int DisposeCallCount { get; private set; } = 0;

        public int DoWorkCallCount { get; private set; } = 0;

        public void DoWork() => RunIfNotDisposed(() => DoWorkCallCount++);

        protected override void DoDispose() => DisposeCallCount++;
    }
}