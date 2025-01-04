namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Moq;

using SteganographyApp.Common.Injection;

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
            var mockableAttribute = GetMockupAttribute(field)!;

            object instance = CreateMockInstance(mockableAttribute);
            field.SetValue(testFixture, instance);

            object proxyInstance = GetProxyInstance(instance);
            UseInstance(proxyInstance, mockableAttribute);
        }
    }

    private static void UseInstance(object instance, Mockup mockableAttribute)
    {
        typeof(Injector)
            .GetMethod(nameof(Injector.UseInstance))
            ?.MakeGenericMethod(mockableAttribute.MockType)
            .Invoke(null, [instance]);
    }

    private static object CreateMockInstance(Mockup mockableAttribute)
    {
        var mockType = typeof(Mock<>).MakeGenericType([mockableAttribute.MockType]);
        return Activator.CreateInstance(mockType, [MockBehavior.Strict])
            ?? throw new Exception($"Could not create mock instance for type: [{mockType.Name}]");
    }

    private static object GetProxyInstance(object mock)
        => mock.GetType().GetProperty("Object", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(mock)
        ?? throw new Exception("Could not find Object property on proxy instance.");

    private static IEnumerable<FieldInfo> GetFieldsRequiringMocks(object testFixture)
        => testFixture.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(HasMockupAttribute);

    private static bool HasMockupAttribute(FieldInfo info) => GetMockupAttribute(info) != null;

    private static Mockup? GetMockupAttribute(FieldInfo info)
        => info.GetCustomAttributes(typeof(Mockup), false).FirstOrDefault() as Mockup;
}

/// <summary>
/// Attribute meant to be used with public Moq.Mock fields that require an
/// injected mock value. Any Moq.Mock field in a TestFixture with this attribute
/// present will have an auto-instantiated Moq.Mock instance value provided during the
/// SetUp phase of the test fixture.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class Mockup(Type mockType) : Attribute
{
    public Type MockType { get; private set; } = mockType;
}