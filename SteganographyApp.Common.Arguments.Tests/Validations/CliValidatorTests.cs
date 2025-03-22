namespace SteganographyApp.Common.Arguments.Tests;

using NUnit.Framework;
using SteganographyApp.Common.Arguments.Validation;

[TestFixture]
public class CliValidatorTests
{
    private readonly CliValidator cliValidator = new();

    [Test]
    public void TestValidate()
    {
        var toValidate = new Valid();
        cliValidator.Validate(toValidate);
    }

    [Test]
    public void TestValidateWithInvalidValuesThrowsValidationFailedException()
    {
        var toValidate = new Invalid();
        Assert.Throws(
            typeof(ValidationFailedException),
            () => cliValidator.Validate(toValidate));
    }
}

internal sealed class Valid
{
    [Argument("--should-be-valid")]
    [InRange(0, 20)]
    public int ShouldBeValid = 10;

    [InRange(0, 20)]
    public int ShouldNotBeValidated = 30;
}

internal sealed class Invalid
{
    [Argument("--should-be-valid")]
    [InRange(0, 20)]
    public int ShouldBeValid = 10;

    [Argument("--show-not-be-invalid")]
    [InRange(0, 20)]
    public int ShouldNotBeValid = 30;
}