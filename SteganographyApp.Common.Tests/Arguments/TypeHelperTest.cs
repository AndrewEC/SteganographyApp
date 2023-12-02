namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SteganographyApp.Common.Arguments;
using TypeHelperAlias = SteganographyApp.Common.Arguments.TypeHelper;

[TestFixture]
public class TypeHelperTests
{
    private readonly MemberInfo stringFieldMember;
    private readonly MemberInfo longPropertyMember;

    private TestSubject subject = new();

    public TypeHelperTests()
    {
        ImmutableArray<MemberInfo> members = TypeHelperAlias.GetAllFieldsAndProperties(subject.GetType());
        Assert.AreEqual(2, members.Length);
        stringFieldMember = MemberWithName(nameof(TestSubject.StringField), members);
        longPropertyMember = MemberWithName(nameof(TestSubject.LongProperty), members);
    }

    [SetUp]
    public void SetUp()
    {
        subject = new TestSubject();
    }

    [Test]
    public void TestDeclaredType()
    {
        Assert.AreEqual(typeof(string), TypeHelperAlias.DeclaredType(stringFieldMember));
        Assert.AreEqual(typeof(long), TypeHelperAlias.DeclaredType(longPropertyMember));
        Assert.Throws(typeof(TypeException), () => TypeHelperAlias.DeclaredType(GetVoidMethod(subject, nameof(TestSubject.VoidMethod))));
    }

    [Test]
    public void TestGetValue()
    {
        Assert.AreEqual(subject.StringField, TypeHelperAlias.GetValue(subject, stringFieldMember));
        Assert.AreEqual(subject.LongProperty, TypeHelperAlias.GetValue(subject, longPropertyMember));
        Assert.Throws(typeof(TypeException), () => TypeHelperAlias.DeclaredType(GetVoidMethod(subject, nameof(TestSubject.VoidMethod))));
    }

    [Test]
    public void TestSetValue()
    {
        string newStringValue = "new_value";
        TypeHelperAlias.SetValue(subject, stringFieldMember, newStringValue);
        Assert.AreEqual(subject.StringField, newStringValue);

        long newLongValue = 234;
        TypeHelperAlias.SetValue(subject, longPropertyMember, newLongValue);
        Assert.AreEqual(subject.LongProperty, newLongValue);

        Assert.Throws(typeof(TypeException), () => TypeHelperAlias.SetValue(subject, GetVoidMethod(subject, nameof(TestSubject.VoidMethod)), newLongValue));
    }

    private static MemberInfo GetVoidMethod(TestSubject subject, string methodName)
        => subject.GetType().GetMethod(methodName) ?? throw new Exception($"Method [{methodName}] not found.");

    private static MemberInfo MemberWithName(string name, ImmutableArray<MemberInfo> members)
        => members.Where(member => member.Name == name).FirstOrDefault() ?? throw new Exception($"Member with name [{name}] not found.");
}

internal sealed class TestSubject
{
    public string StringField = "string_field";

    public long LongProperty { get; set; } = 100;

    public void VoidMethod() { }
}