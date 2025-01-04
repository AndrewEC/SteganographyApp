internal static class ChunkTableConstants
{
    /// <summary>
    /// The number of iterations to multiplicatively apply when hashing
    /// any random seeds or passwords.
    /// </summary>
    public static readonly int IterationMultiplier = 1000;

    /// <summary>
    /// The number of dummy entries to be inserted into the chunk table.
    /// This value must always be 0 as it is expected that the chunk table header
    /// be of a fixed size.
    /// </summary>
    public static readonly int DummyCount = 0;
}