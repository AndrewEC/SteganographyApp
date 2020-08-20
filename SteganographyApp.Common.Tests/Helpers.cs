using Moq;

using NUnit.Framework;

using System;
using System.Linq;
using System.Reflection;

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

        [SetUp]
        public void InitializeMocks()
        {
            TestInjectionHelper.InjectMocks(this);
            SetupMocks();
        }

        protected virtual void SetupMocks() {}

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

    [AttributeUsage(AttributeTargets.Field)]
    public class InjectMockAttribute : Attribute
    {

        public Type MockType { get; private set; }

        public InjectMockAttribute(Type mockType)
        {
            MockType = mockType;
        }

    }

    public static class TestInjectionHelper
    {

        public static void InjectMocks(object testFixture)
        {
            foreach (var field in GetFieldsRequiringMocks(testFixture))
            {
                var mockableAttribute = GetInjectMockAttribute(field);

                object instance = CreateMockInstance(field, mockableAttribute);
                field.SetValue(testFixture, instance);

                object proxyInstance = GetProxyInstance(instance);
                UseInstance(proxyInstance, field, mockableAttribute);
            }
        }

        private static void UseInstance(object instance, FieldInfo fieldInfo, InjectMockAttribute mockableAttribute)
        {
            typeof(Injector)
                .GetMethod("UseInstance")
                .MakeGenericMethod(mockableAttribute.MockType)
                .Invoke(null, new object[] { instance });
        }

        private static object CreateMockInstance(FieldInfo field, InjectMockAttribute mockableAttribute)
        {
            var mockType = typeof(Mock<>).MakeGenericType(new Type[] { mockableAttribute.MockType });
            return Activator.CreateInstance(mockType);
        }

        private static object GetProxyInstance(object mock)
        {
            return mock.GetType().GetProperty("Object", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetValue(mock);
        }

        private static FieldInfo[] GetFieldsRequiringMocks(object testFixture)
        {
            return testFixture.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(HasInjectMockAttribute).ToArray();
        }

        private static bool HasInjectMockAttribute(FieldInfo info)
        {
            return GetInjectMockAttribute(info) != null;
        }

        private static InjectMockAttribute GetInjectMockAttribute(FieldInfo info)
        {
            return info.GetCustomAttributes(typeof(InjectMockAttribute), false).FirstOrDefault() as InjectMockAttribute;
        }
    }

}