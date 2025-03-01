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
        foreach (FieldInfo field in GetFieldsRequiringMocks(testFixture))
        {
            Mockup mockableAttribute = GetMockupAttribute(field)!;

            object instance = CreateMockInstance(mockableAttribute);
            field.SetValue(testFixture, instance);

            if (!mockableAttribute.SkipInjection)
            {
                UseInstance(((Mock)instance).Object, mockableAttribute);
            }
        }
    }

    private static void UseInstance(object instance, Mockup mockableAttribute)
        => typeof(Injector)
            .GetMethod(nameof(Injector.UseInstance))
            ?.MakeGenericMethod(mockableAttribute.MockType)
            .Invoke(null, [instance]);

    private static object CreateMockInstance(Mockup mockableAttribute)
    {
        Type mockType = typeof(Mock<>).MakeGenericType([mockableAttribute.MockType]);
        return Activator.CreateInstance(mockType, [MockBehavior.Strict])
            ?? throw new Exception($"Could not create mock instance for type: [{mockType.Name}]");
    }

    private static IEnumerable<FieldInfo> GetFieldsRequiringMocks(object testFixture)
        => testFixture.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(fieldInfo => GetMockupAttribute(fieldInfo) != null);

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
public class Mockup(Type mockType, bool skipInjection = false) : Attribute
{
    public Type MockType { get; } = mockType;

    public bool SkipInjection { get; } = skipInjection;
}