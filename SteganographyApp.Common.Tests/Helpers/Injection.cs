using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Moq;

using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    /// <summary>
    /// Attribute meant to be used with public Moq.Mock fields that require an
    /// injected mock value. Any Moq.Mock field in a TestFixture with this attribute
    /// present will have an auto-instantiated Moq.Mock instance value provided during the
    /// SetUp phase of the test fixture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class Mockup : Attribute
    {

        public Type MockType { get; private set; }

        public Mockup(Type mockType)
        {
            MockType = mockType;
        }

    }

    /// <summary>
    /// Static helper class to reflectively lookup public Moq.Mock fields in a given TestFixture class
    /// that are annotated with the InjectMock attribute and set their value to a given auto-instantiated instance
    /// while also updating the Injector instances to use the new mock instance as the value in place of a given
    /// interface.
    /// </summary>
    public static class MocksInjector
    {

        public static void InjectMocks(object testFixture)
        {
            foreach (var field in GetFieldsRequiringMocks(testFixture))
            {
                var mockableAttribute = GetMockupAttribute(field);

                object instance = CreateMockInstance(field, mockableAttribute);
                field.SetValue(testFixture, instance);

                object proxyInstance = GetProxyInstance(instance);
                UseInstance(proxyInstance, field, mockableAttribute);
            }
        }

        private static void UseInstance(object instance, FieldInfo fieldInfo, Mockup mockableAttribute)
        {
            typeof(Injector)
                .GetMethod("UseInstance")
                .MakeGenericMethod(mockableAttribute.MockType)
                .Invoke(null, new object[] { instance });
        }

        private static object CreateMockInstance(FieldInfo field, Mockup mockableAttribute)
        {
            var mockType = typeof(Mock<>).MakeGenericType(new Type[] { mockableAttribute.MockType });
            return Activator.CreateInstance(mockType);
        }

        private static object GetProxyInstance(object mock)
        {
            return mock.GetType().GetProperty("Object", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetValue(mock);
        }

        private static IEnumerable<FieldInfo> GetFieldsRequiringMocks(object testFixture)
        {
            return testFixture.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(HasMockupAttribute);
        }

        private static bool HasMockupAttribute(FieldInfo info)
        {
            return GetMockupAttribute(info) != null;
        }

        private static Mockup GetMockupAttribute(FieldInfo info)
        {
            return info.GetCustomAttributes(typeof(Mockup), false).FirstOrDefault() as Mockup;
        }
    }

}