namespace SteganographyApp.Common.IO
{
    using SixLabors.ImageSharp.PixelFormats;

    /// <summary>
    /// Functional interface specifying a single method to read a set of bits from a
    /// queue and use said bits to identify a new colour value.
    /// </summary>
    internal interface IColourChannelUpdateStrategy
    {
        /// <summary>
        /// Reads a set of bits from the queue and overwriter the lower bits of the source colour
        /// with them to determine the new bit colour.
        /// </summary>
        /// <param name="sourceColour">The byte representation of a pixel colour channel.</param>
        /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
        /// <returns>The newly formed colour of the input source colour.</returns>
        byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue);
    }

    /// <summary>
    /// Static utility class to assist in performing some bitwise operations.
    /// </summary>
    internal static class Bitwise
    {
        /// <summary>
        /// Swaps the least significant bit in the input value byte with the input binary character.
        /// </summary>
        /// <param name="value">The input byte whose least significant bit will be swapped with the input bit.</param>
        /// <param name="lastBit">The character specifiying a bit like value, 0 or 1, to swap in.</param>
        /// <returns>The newly formed byte.</returns>
        internal static byte SwapLeastSigificantBit(byte value, char lastBit) => Shift(value, 1, lastBit);

        /// <summary>
        /// Swaps the second least significant bit in the input value byte with the input binary character.
        /// </summary>
        /// <param name="value">The input byte whose second least significant bit will be swapped with the input bit.</param>
        /// <param name="lastBit">The character specifying a bit like value, 0 or 1, to swap in.</param>
        /// <returns>The newly formed byte.</returns>
        internal static byte SwapSecondLeastSignificantBit(byte value, char lastBit) => Shift(value, 2, lastBit);
        
        private static byte Shift(byte value, byte shift, char lastBit)  => (lastBit == '0')
            ? (byte)(value & ~shift)
            : (byte)(value | shift);
    }

    /// <summary>
    /// A colour channel update strategy intended to update only the least significant bit of a give input byte.
    /// </summary>
    internal sealed class SingleBitColourChannelUpdateStrategy : IColourChannelUpdateStrategy
    {
        /// <summary>
        /// Reads a set of bits from the queue and overwriter the lower bits of the source colour
        /// with them to determine the new bit colour.
        /// </summary>
        /// <param name="sourceColour">The byte representation of a pixel colour channel.</param>
        /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
        /// <returns>The newly formed colour of the input source colour.</returns>
        public byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue)
        {
            if (!bitQueue.HasNext())
            {
                return sourceColour;
            }
            return Bitwise.SwapLeastSigificantBit(sourceColour, bitQueue.Next());
        }
    }

    /// <summary>
    /// A colour channel update strategy intended to update the least significant and second-least significant bit of a give input byte.
    /// </summary>
    internal sealed class TwoBitColourChannelUpdateStrategy : IColourChannelUpdateStrategy
    {
        /// <summary>
        /// Reads a set of bits from the queue and overwriter the lower bits of the source colour
        /// with them to determine the new bit colour.
        /// </summary>
        /// <param name="sourceColour">The byte representation of a pixel colour channel.</param>
        /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
        /// <returns>The newly formed colour of the input source colour.</returns>
        public byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue)
        {
            if (!bitQueue.HasNext())
            {
                return sourceColour;
            }
            byte destinationColour = Bitwise.SwapSecondLeastSignificantBit(sourceColour, bitQueue.Next('0'));
            destinationColour = Bitwise.SwapLeastSigificantBit(destinationColour, bitQueue.Next('0'));
            return destinationColour;
        }
    }

    /// <summary>
    /// Utility class to read in binary data from a pixel and write it to a bit queue.
    /// </summary>
    internal sealed class PixelWriter
    {
        private readonly ReadBitQueue bitQueue;
        private readonly IColourChannelUpdateStrategy strategy;

        /// <summary>
        /// Constructs an instance of the pixel writer.
        /// </summary>
        /// <param name="bitQueue">The queue to write the read bits to.</param>
        /// <param name="writableBitsPerPixel">The number of bits to read, starting from the LSB, from a given input pixels
        /// RGB colours.</param>
        public PixelWriter(ReadBitQueue bitQueue, int writableBitsPerPixel)
        {
            this.bitQueue = bitQueue;
            strategy = (writableBitsPerPixel == 2) ? new TwoBitColourChannelUpdateStrategy() : new SingleBitColourChannelUpdateStrategy();
        }

        /// <summary>
        /// Performs an in place mutation of the input pixel struct by updating the LSBs of the
        /// RGB values from the input pixel.
        /// </summary>
        /// <param name="source">The input pixel the original RGB bytes to update.</param>
        /// <returns>A new pixel with each RGB value being an RGB valued pulled from the source pixel that
        /// has been subsequently updated by pulling a bit from the ReadBitQueue instance provided during initialization.</returns>
        public Rgba32 UpdatePixel(Rgba32 source)
        {
            var destination = new Rgba32();
            destination.R = strategy.GetNewPixelColour(source.R, bitQueue);
            destination.G = strategy.GetNewPixelColour(source.G, bitQueue);
            destination.B = strategy.GetNewPixelColour(source.B, bitQueue);
            return destination;
        }
    }
}