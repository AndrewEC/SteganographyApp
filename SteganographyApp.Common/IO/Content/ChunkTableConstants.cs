namespace SteganographyApp.Common.IO.Content;

/// <summary>
/// A shared constant class to be used between the various chunk table
/// implementations.
/// </summary>
internal static class ChunkTableConstants
{
    /// <summary>
    /// The number of iterations to multiplicatively apply when hashing
    /// any random seeds or passwords.
    /// </summary>
    public static readonly int IterationMultiplier = 1000;
}