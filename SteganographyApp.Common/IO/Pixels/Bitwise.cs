namespace SteganographyApp.Common.IO.Pixels;

/// <summary>
/// Static utility class to assist in performing some bitwise operations.
/// </summary>
internal static class Bitwise
{
    /// <summary>
    /// Swaps the least significant bit in the input value byte with the input binary character.
    /// </summary>
    /// <param name="value">The input byte whose least significant bit will be swapped
    /// with the input bit.</param>
    /// <param name="lastBit">The character specifiying a bit like value, 0 or 1,
    /// to swap in.</param>
    /// <returns>The newly formed byte.</returns>
    internal static byte SwapLeastSigificantBit(byte value, char lastBit)
        => Swap(value, 1, lastBit);

    /// <summary>
    /// Swaps the second least significant bit in the input value byte with the
    /// input binary character.
    /// </summary>
    /// <param name="value">The input byte whose second least significant bit
    /// will be swapped with the input bit.</param>
    /// <param name="lastBit">The character specifying a bit like value,
    /// 0 or 1, to swap in.</param>
    /// <returns>The newly formed byte.</returns>
    internal static byte SwapSecondLeastSignificantBit(byte value, char lastBit)
        => Swap(value, 2, lastBit);

    private static byte Swap(byte value, byte shift, char lastBit) => (lastBit == '0')
        ? (byte)(value & ~shift)
        : (byte)(value | shift);
}