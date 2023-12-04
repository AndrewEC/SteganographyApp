namespace SteganographyApp.Common.Arguments.Tests;

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
        Assert.That(members, Has.Length.EqualTo(2));
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
        Assert.Multiple(() =>
        {
            Assert.That(TypeHelperAlias.GetDeclaredType(stringFieldMember), Is.EqualTo(typeof(string)));
            Assert.That(TypeHelperAlias.GetDeclaredType(longPropertyMember), Is.EqualTo(typeof(long)));
        });
        Assert.Throws(typeof(TypeException), () => TypeHelperAlias.GetDeclaredType(GetVoidMethod(subject, nameof(TestSubject.VoidMethod))));
    }

    [Test]
    public void TestGetValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TypeHelperAlias.GetValue(subject, stringFieldMember), Is.EqualTo(subject.StringField));
            Assert.That(TypeHelperAlias.GetValue(subject, longPropertyMember), Is.EqualTo(subject.LongProperty));
        });
        Assert.Throws(typeof(TypeException), () => TypeHelperAlias.GetDeclaredType(GetVoidMethod(subject, nameof(TestSubject.VoidMethod))));
    }

    [Test]
    public void TestSetValue()
    {
        string newStringValue = "new_value";
        TypeHelperAlias.SetValue(subject, stringFieldMember, newStringValue);
        Assert.That(newStringValue, Is.EqualTo(subject.StringField));

        long newLongValue = 234;
        TypeHelperAlias.SetValue(subject, longPropertyMember, newLongValue);
        Assert.That(newLongValue, Is.EqualTo(subject.LongProperty));

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