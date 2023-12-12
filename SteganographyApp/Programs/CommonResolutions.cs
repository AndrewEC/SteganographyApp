namespace SteganographyApp;

using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// An enumeration of the commonly available resolutions.
/// </summary>
public enum CommonResolutions
{
    P360,
    P480,
    P720,
    P1080,
    P1440,
    P2160,
}

/// <summary>
/// A static class that provides quick access to the display name and dimensions of the
/// various <see cref="CommonResolutions"/> values.
/// </summary>
public static class Resolution
{
    static Resolution()
    {
        Dictionary<CommonResolutions, (int, int)> dimensions = [];
        dimensions.Add(CommonResolutions.P360, (480, 360));
        dimensions.Add(CommonResolutions.P480, (640, 480));
        dimensions.Add(CommonResolutions.P720, (1280, 720));
        dimensions.Add(CommonResolutions.P1080, (1920, 1080));
        dimensions.Add(CommonResolutions.P1440, (2560, 1440));
        dimensions.Add(CommonResolutions.P2160, (3840, 2160));

        Dimensions = dimensions.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets the dimensions, width and height, associated with each of the common resolutions
    /// specified in the <see cref="CommonResolutions" enum./>.
    /// </summary>
    public static ImmutableDictionary<CommonResolutions, (int Width, int Height)> Dimensions { get; }

    /// <summary>
    /// Maps the input resolution into a more user friendly name.
    /// <para>For example the resolution <see cref="CommonResolutions.P360"/> would be mapped to 360p.</para>
    /// </summary>
    /// <param name="resolution">The resolution enum value to be mapped to a displayable format.</param>
    /// <returns>A string representation of the resolution.</returns>
    public static string ToDisplayName(CommonResolutions resolution) => resolution.ToString().Replace("P", string.Empty) + 'p';
}