namespace SteganographyApp.Common.Arguments.Validation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// A validation attribute used to validate that a given numerical field or property
/// has a value that is between a min and a max value inclusively.
/// </summary>
/// <param name="min">The minimum value the underlying field or property can be.</param>
/// <param name="max">The maximum value the underlying field or property can be.</param>
/// <param name="name">An optional name for this validatin attribute instance. This can provide more
/// informatiom if an exception is thrown an potentially help narrow down the source of an error
/// if multiple InRangeAttributes are being used in a single program.</param>
public class InRangeAttribute(double min, double max, string? name = null) : ValidationAttribute(name, AllNumberTypes)
{
    private static readonly CompositeFormat ValidationFailedFormat = CompositeFormat.Parse(
        "Expected value to be between [{0}] and [{1}] but instead was [{2}].");

    private static readonly ImmutableArray<Type> WholeNumberTypes = [
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(long)
    ];

    private static readonly ImmutableArray<Type> DecimalNumberTypes = [
        typeof(float),
        typeof(decimal),
        typeof(double)
    ];

    private static readonly IEnumerable<Type> AllNumberTypes = WholeNumberTypes.Concat(DecimalNumberTypes);

    private readonly double min = min;
    private readonly double max = max;

    /// <summary>
    /// Performs validation on the value of the attributed field or property to ensure it is within the specified range.
    /// <para>It can be assumed that the value is not null and that the value is a numeric type of either: byte,
    /// short, int, long, float, decimal, or double.</para>
    /// </summary>
    /// <param name="value">The value of the underlying field or property.</param>
    /// <exception cref="ValidationFailedException">Thrown if the input value is not between or equal to the specified min
    /// and max values.</exception>
    protected override void DoValidate(object value)
    {
        if (!IsInRange(value))
        {
            throw new ValidationFailedException(
                string.Format(CultureInfo.InvariantCulture, ValidationFailedFormat, min, max, value));
        }
    }

    private bool IsInRange(object value)
    {
        if (WholeNumberTypes.Contains(value.GetType()))
        {
            long longValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            return longValue >= min && longValue <= max;
        }

        double doubleValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        return doubleValue >= min && doubleValue <= max;
    }
}